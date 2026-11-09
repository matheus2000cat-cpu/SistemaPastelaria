# Sistema da Pastelaria

Aplicação ASP.NET Core MVC para pastelarias, com cardápio público, pedidos para retirada/entrega e painel de operação. Usa .NET 10 LTS e Supabase. O login de funcionários protege a área administrativa.

## Preparar o Supabase

1. Abra o SQL Editor do projeto Supabase e execute [`supabase_schema.sql`](./supabase_schema.sql).
2. No Render, configure `Supabase__Url` e `Supabase__Key` nas variáveis privadas do serviço. A chave fica somente no servidor — nunca a coloque no JavaScript ou em páginas públicas.
3. Use uma chave de servidor com permissão para ler e escrever nas tabelas. A aplicação valida os dados do pedido no servidor e usa o login próprio de funcionários para proteger as ações administrativas. Nunca use a chave `anon` para escrita administrativa.
4. Cadastre os produtos pelo painel da equipe depois de entrar com um funcionário ativo.
5. Se o schema `operario` ainda não tiver funcionários ativos, cadastre a equipe antes de usar o painel.
6. Este script é aditivo para as tabelas `produto` e `pedido` que já existirem: ele acrescenta as colunas de catálogo e pedido online sem remover os campos usados pelo PDV e pela cozinha.
7. Nunca publique PINs, chaves ou resultados brutos de consultas em páginas de depuração.

## Identidade da pastelaria

Edite `Branding:Name` e `Branding:Tagline` em `appsettings.json` para personalizar o nome e a frase da loja. Também é possível definir `Branding__Name` e `Branding__Tagline` como variáveis de ambiente no Render, sem alterar os arquivos do projeto.

## Executar localmente

Instale o SDK do .NET 10, configure `Supabase__Url` e `Supabase__Key` no ambiente e execute:

```powershell
dotnet run
```

## Render

O [`Dockerfile`](./Dockerfile) prepara uma imagem ASP.NET Core 10, escuta na porta `10000` e inclui a rota de saúde `/healthz`.

Para publicar, o repositório precisa estar conectado a um Web Service Docker no Render. Configure `Supabase__Url` e `Supabase__Key` como variáveis secretas no painel do serviço; opcionalmente defina `Branding__Name` e `Branding__Tagline`. Depois envie as alterações para o branch conectado ao Render e acompanhe o deploy e a verificação de saúde. Se o serviço atual não for Docker, crie um Web Service Docker conectado ao repositório ou migre o serviço no painel antes do deploy.

O endpoint `/healthz` confirma que a aplicação iniciou; ele não verifica a conexão com o Supabase. Pedidos são registrados sem cobrança online. A equipe confirma o pedido e recebe o pagamento na loja ou na entrega.
