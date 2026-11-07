terraform {
  backend "gcs" {
    bucket = "vcosta-fiap-tech-challenge-tfstate"
    prefix = "ordem-servico-service"
  }
}
