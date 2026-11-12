data "google_project" "current" {
  project_id = var.project_id
}

locals {
  runtime_sa_email = data.terraform_remote_state.bootstrap.outputs.runtime_service_account_emails["ordem-servico-service"]
  pubsub_agent     = "serviceAccount:service-${data.google_project.current.number}@gcp-sa-pubsub.iam.gserviceaccount.com"

  consume = {
    "ordem-servico-orcamento-gerado"     = "orcamento-gerado"
    "ordem-servico-orcamento-falhou"     = "orcamento-falhou"
    "ordem-servico-estoque-reservado"    = "estoque-reservado"
    "ordem-servico-estoque-em-falta"     = "estoque-em-falta"
    "ordem-servico-cobranca-gerada"      = "cobranca-gerada"
    "ordem-servico-pagamento-confirmado" = "pagamento-confirmado"
    "ordem-servico-pagamento-recusado"   = "pagamento-recusado"
  }

  publish = toset([
    "diagnostico-finalizado",
    "ordem-servico-aprovada",
    "ordem-servico-finalizada",
    "ordem-servico-descartada",
  ])
}

resource "google_pubsub_topic" "dlq" {
  for_each = local.consume
  name     = "${each.key}-dlq"
  project  = var.project_id

  message_storage_policy {
    allowed_persistence_regions = [var.region]
  }
}

resource "google_pubsub_subscription" "pull" {
  for_each = local.consume
  name     = each.key
  topic    = each.value
  project  = var.project_id

  ack_deadline_seconds = 30

  retry_policy {
    minimum_backoff = "10s"
    maximum_backoff = "600s"
  }

  dead_letter_policy {
    dead_letter_topic     = google_pubsub_topic.dlq[each.key].id
    max_delivery_attempts = 5
  }
}

resource "google_pubsub_topic_iam_member" "publisher" {
  for_each = local.publish
  project  = var.project_id
  topic    = each.value
  role     = "roles/pubsub.publisher"
  member   = "serviceAccount:${local.runtime_sa_email}"
}

resource "google_pubsub_subscription_iam_member" "subscriber" {
  for_each     = local.consume
  project      = var.project_id
  subscription = google_pubsub_subscription.pull[each.key].name
  role         = "roles/pubsub.subscriber"
  member       = "serviceAccount:${local.runtime_sa_email}"
}

resource "google_pubsub_topic_iam_member" "dlq_pubsub_agent_publisher" {
  for_each = local.consume
  project  = var.project_id
  topic    = google_pubsub_topic.dlq[each.key].name
  role     = "roles/pubsub.publisher"
  member   = local.pubsub_agent
}

resource "google_pubsub_subscription_iam_member" "dlq_pubsub_agent_subscriber" {
  for_each     = local.consume
  project      = var.project_id
  subscription = google_pubsub_subscription.pull[each.key].name
  role         = "roles/pubsub.subscriber"
  member       = local.pubsub_agent
}
