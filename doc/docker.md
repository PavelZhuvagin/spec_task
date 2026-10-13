# CSG docker

## 1. docker yml

- Порядок: как в примере ниже

```yaml
version: '3.4'

services:
  pg_db:
    container_name:
    image: git.rt-7.ru/ris/dependency_proxy/containers/postgres:12.4
    restart: always
    privileged: true
    ports:
    environment:
    volumes:
    build:
      context: .
      dockerfile: ./Dockerfile
    depends_on:
    secrets:
    command:
    entrypoint:
    networks:
    runtime: nvidia # Эта строка обязательна. Она указывает Docker использовать NVIDIA runtime для этого контейнера.
      deploy:
        resources:
          reservations:
            devices: # В этом блоке описывается запрос на GPU-ресурсы.
              - driver: nvidia # Указывает драйвер для NVIDIA GPU.
                # Это обязательное поле, которое говорит Docker, что нам нужна именно возможность "gpu".
                # [gpu] - минимально, utility - для nvidia-smi
                capabilities: [gpu, utility]
                # Важно: count и device_ids нельзя использовать одновременно.
                count: 1                      # Запрашивает 1 GPU. Можно использовать count: all, чтобы дать контейнеру доступ ко всем GPU на хосте.
                # ИЛИ device_ids: ['0']       # Альтернатива count. Позволяет привязать конкретную видеокарту по её ID (номер можно узнать из nvidia-smi).

secrets:
  orthanc.json:
    file: ./docker/orthanc.json

volumes:
  risvolume:
    driver: local
    driver_opts:
      type: 'none'
      o: 'bind'
      device: 'C:\\Users\\User\\Desktop\\Нужное\\RIS-develop\\RIS'

networks:
  ris-network:
    driver: bridge
```
