# Datadog — `ordem-servico-service`

Site: `us5.datadoghq.com`. Dashboard geral da Saga: repo `e2e` → `docs/datadog/`.

## Arquivos

| Arquivo | Uso |
|---------|-----|
| [`dashboard-service-demo.json`](dashboard-service-demo.json) | APM/logs deste serviço (`DD_SERVICE=ordem-servico-service`) |
| [`monitor-erros-http.json`](monitor-erros-http.json) | Erros APM HTTP |
| [`monitor-health-uptime.json`](monitor-health-uptime.json) | Hits em `GET /ordem-servico/health` (`notify_no_data: false`) |
| [`monitor-falhas-os.json`](monitor-falhas-os.json) | Logs `status:error` ligados a OS |

## Importar

1. Abra [https://us5.datadoghq.com](https://us5.datadoghq.com).
2. **Dashboards** → **Import dashboard JSON** → [`dashboard-service-demo.json`](dashboard-service-demo.json).
3. **Monitors** → importe cada `monitor-*.json`. Destino de alerta só na UI (não versionar e-mail).
4. Após `demo-down`, mute os monitores.

## Links úteis

| Recurso | URL |
|---------|-----|
| APM — `ordem-servico-service` | https://us5.datadoghq.com/apm/entity/service%3Aordem-servico-service |
| Health (GCP) | https://vcosta-fiap.online/ordem-servico/health |

Logs JSON com `DD_LOGS_INJECTION=true` levam `dd.trace_id` → **View Trace**.
