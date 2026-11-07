output "database_name" {
  description = "Nome do database PostgreSQL deste serviço."
  value       = google_sql_database.service.name
}

output "db_user" {
  description = "Usuário SQL da aplicação (role ordem_servico_app)."
  value       = google_sql_user.app.name
}

output "connection_name" {
  description = "Connection name da instância Cloud SQL compartilhada."
  value       = data.terraform_remote_state.infra_db.outputs.connection_name
}

output "connection_string_secret_id" {
  description = "Secret Manager id da connection string (Host=127.0.0.1 via Auth Proxy)."
  value       = google_secret_manager_secret.connection_string.secret_id
}

