# O que é

=====================

Esse projeto é uma PoC que:

- Utiliza o Debezium para escutar as alterações no banco de dados;
- Envia essas alterações para um tópico do kafka;
- API em .Net Core 3 que fica escutando esse tópico e atualizando o cache em memória;
- Endpoint da API devolver a informação que está em cache.

Em geral, a ideia é provar que a API retorna sempre a última informação de um determinado id de produto sem qualquer comunicação (direta) com o banco de dados.

## Tecnologias

- .NET Core 3 (Api com MemoryCache)
- Kafka (confluentinc)
- Debezium

## Banco de dados implementado na PoC

- MySql

## Requisitos de dev

- VS ou VS Code
- .NET Core SDK 3
- Docker (com docker compose)

## Como executar

- Na pasta raiz, executar:

```bash
docker-compose up
```

- Abra um outro terminal e vamos criar uma tabela de produtos no mysql e inserir um registro:

```bash
docker-compose exec mysql bash -c "mysql -u root -p$MYSQL_ROOT_PASSWORD"

create database mystore;

use mystore;

create table products (id int unsigned auto_increment primary key, name varchar(50), price int, creation_time datetime default current_timestamp, modification_time datetime on update current_timestamp);

insert into products(name, price) values("Red T-Shirt", 12);
```

- Para escutar todas as alterações que ocorrem na tabela, precisamos criar um **conector**;
- Conector é um aplicativo responsável por mover dados de um banco de dados para o cluster Kafka. É aqui que entra o Debezium;
- **Importante**: perceba que no compose estou passando no volume do mysql o *my.cnf*. Esse arquivo tem a configuração para habilitar o CDC do mysql;
- Abra um outro terminal e vamos executar:

```bash
curl -i -X POST -H "Accept:application/json" -H "Content-Type:application/json" localhost:8083/connectors/ -d '{ "name": "mystore-connector", "config": { "connector.class": "io.debezium.connector.mysql.MySqlConnector", "tasks.max": "1", "database.hostname": "mysql", "database.port": "3306", "database.user": "root", "database.password": "root", "database.server.id": "223345", "database.server.name": "mysql", "database.whitelist": "mystore", "database.history.kafka.bootstrap.servers": "kafka:9092", "database.history.kafka.topic": "dbhistory.mystore",
"transforms":"unwrap","transforms.unwrap.type":"io.debezium.transforms.UnwrapFromEnvelope","transforms.unwrap.drop.tombstones":"false","key.converter": "org.apache.kafka.connect.json.JsonConverter","key.converter.schemas.enable": "false","value.converter": "org.apache.kafka.connect.json.JsonConverter","value.converter.schemas.enable": "false","include.schema.changes": "false"} }'
```

- A resposta tem que ser *HTTP/1.1 201 Created*. Para verificar o status: *curl localhost:8083/connectors/mystore-connector/status*.

- Para verificarmos que as mensagens estão chegando no kafka, abra um novo terminal e execute:

```bash
docker-compose exec kafka bash

kafka-console-consumer --bootstrap-server kafka:9092 --from-beginning --topic mysql.mystore.products --property print.key=true --property key.separator="-"
```

- Agora vamos voltar ao terminal do mysql e executar algumas queries (em paralelo para cada query executada, vamos vendo como está se comportando o terminal do kafka):

```bash
insert into products(name, price) values("Blue Hat", 5);

update products set price = 17 where name = "Blue Hat";

delete from products where id = 1;

alter table products add column description nvarchar(1000);

update products set description = "Hello world!" where id = 2;
```

- Se tudo deu certo, no termial do kafka foi printando as mensagens que o debezium foi enviando para o kafka conforme a execução das queries.
- Agora vamos para a API que está rodando

> *Esse código foi baseado no artigo do Seyed Morteza Mousavi com algumas diferenças de tecnologias e implementações.*
