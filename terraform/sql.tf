locals {
  db_instance_name = var.db_instance_name
  db_name          = "ordem_servico"
  db_user          = "ordem_servico"
  provisioner_user = "ordem_servico_provisioner"
}

resource "google_sql_database_instance" "main" {
  name             = local.db_instance_name
  database_version = "POSTGRES_16"
  region           = var.region
  project          = var.project_id

  settings {
    tier              = var.db_tier
    availability_type = "ZONAL"
    disk_type         = "PD_SSD"
    disk_size         = 10
    edition           = "ENTERPRISE"
    data_api_access   = "ALLOW_DATA_API"

    database_flags {
      name  = "log_checkpoints"
      value = "on"
    }

    database_flags {
      name  = "log_connections"
      value = "on"
    }

    database_flags {
      name  = "log_disconnections"
      value = "on"
    }

    database_flags {
      name  = "log_lock_waits"
      value = "on"
    }

    database_flags {
      name  = "log_temp_files"
      value = "0"
    }

    ip_configuration {
      ipv4_enabled                                  = false
      private_network                               = data.terraform_remote_state.bootstrap.outputs.network_id
      enable_private_path_for_google_cloud_services = true
      ssl_mode                                      = "ENCRYPTED_ONLY"
    }

    backup_configuration {
      enabled = false
    }
  }

  deletion_protection = false
}

resource "random_password" "provisioner" {
  length  = 32
  special = false
}

resource "random_password" "app" {
  length  = 32
  special = false
}

resource "google_sql_database" "service" {
  name     = local.db_name
  instance = google_sql_database_instance.main.name
  project  = var.project_id
}

resource "google_sql_user" "provisioner" {
  name     = local.provisioner_user
  instance = google_sql_database_instance.main.name
  project  = var.project_id
  password = random_password.provisioner.result
}

resource "google_secret_manager_regional_secret" "provisioner_password" {
  secret_id = "ordem-servico-provisioner-password"
  location  = var.region
  project   = var.project_id

  labels = {
    service = "ordem-servico-service"
    purpose = "data-api"
  }
}

resource "google_secret_manager_regional_secret_version" "provisioner_password" {
  secret      = google_secret_manager_regional_secret.provisioner_password.id
  secret_data = random_password.provisioner.result
}

resource "google_sql_user" "app" {
  name     = local.db_user
  instance = google_sql_database_instance.main.name
  project  = var.project_id
  password = random_password.app.result

  depends_on = [google_sql_database.service]
}

resource "google_sql_provision_script" "grants" {
  instance    = google_sql_database_instance.main.name
  database    = google_sql_database.service.name
  description = "Grants schema public para o usuário da app"
  script      = file("${path.module}/grant.sql")
  user        = google_sql_user.provisioner.name

  password_secret_version = google_secret_manager_regional_secret_version.provisioner_password.name

  depends_on = [
    google_sql_user.provisioner,
    google_sql_user.app,
    google_secret_manager_regional_secret_version.provisioner_password,
  ]
}
