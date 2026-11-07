data "terraform_remote_state" "infra_db" {
  backend = "gcs"

  config = {
    bucket = "vcosta-fiap-tech-challenge-tfstate"
    prefix = "infra-db"
  }
}
