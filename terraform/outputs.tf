output "database_name" {
  description = "Nome do database PostgreSQL deste serviço."
  value       = google_sql_database.service.name
}

output "db_user" {
  description = "Usuário SQL da aplicação."
  value       = google_sql_user.app.name
}

output "instance_name" {
  description = "Nome da instância Cloud SQL deste serviço."
  value       = google_sql_database_instance.main.name
}

output "connection_name" {
  description = "Connection name do Cloud SQL (project:region:instance)."
  value       = google_sql_database_instance.main.connection_name
}

output "connection_string_secret_id" {
  description = "Secret Manager id da connection string (Host=127.0.0.1 via Auth Proxy)."
  value       = google_secret_manager_secret.connection_string.secret_id
}
