resource "random_password" "service_auth" {
  length  = 48
  special = false
}

locals {
  connection_string = join(";", [
    "Host=127.0.0.1",
    "Port=5432",
    "Database=${local.db_name}",
    "Username=${local.db_user}",
    "Password=${random_password.app.result}",
    "Maximum Pool Size=15",
  ])
}

resource "google_secret_manager_secret" "connection_string" {
  secret_id = "ordem-servico-service-connection-string"
  project   = var.project_id

  labels = {
    service = "ordem-servico-service"
  }

  replication {
    auto {}
  }
}

resource "google_secret_manager_secret_version" "connection_string" {
  secret      = google_secret_manager_secret.connection_string.id
  secret_data = local.connection_string
}

resource "google_secret_manager_secret" "service_auth" {
  secret_id = "ordem-servico-service-service-auth-key"
  project   = var.project_id

  labels = {
    service = "ordem-servico-service"
  }

  replication {
    auto {}
  }
}

resource "google_secret_manager_secret_version" "service_auth" {
  secret      = google_secret_manager_secret.service_auth.id
  secret_data = random_password.service_auth.result
}
