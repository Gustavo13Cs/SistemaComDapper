# 🚗 Sistema de Estacionamento

Bem-vindo ao **Sistema de Estacionamento**, uma aplicação moderna e responsiva desenvolvida em **C#** com **Dapper** e **MySQL**, que permite o gerenciamento completo de um estacionamento com atualização em tempo real de vagas e tickets.

<div align="center">
  <img src="caminho/para/sua/imagem1.png" width="800"/>
</div>

---

## 📌 Funcionalidades Principais

- 👥 **Cadastro de Clientes** com busca por nome ou CPF
- 🚘 **Cadastro de Veículos**
- 🎫 **Criação de Tickets** com entrada, saída e pagamento
- 🅿️ **Controle de Vagas** totalmente automatizado
- 💰 **Valor por Minuto** configurável diretamente na interface
- 🔄 **Atualização em tempo real** dos estados dos tickets e vagas
- ✅ Liberação automática de vagas após o pagamento do ticket

---

## 💻 Tecnologias Utilizadas

- **Linguagem:** C#
- **Backend:** ASP.NET Core
- **ORM:** [Dapper](https://github.com/DapperLib/Dapper)
- **Banco de Dados:** MySQL
- **Frontend:** Razor Pages / HTML + CSS
- **Estilo:** Bootstrap (com dark theme customizado)
- **Arquitetura:** MVC simplificado

---

## 🖼️ Prints da Interface

### 🎛️ Dashboard Inicial

<img src="caminho/para/sua/imagem1.png" width="800"/>

### 👥 Gerenciamento de Clientes

<img src="caminho/para/sua/imagem2.png" width="800"/>

### 🎫 Tickets de Estacionamento

<img src="caminho/para/sua/imagem3.png" width="800"/>

### 💳 Ticket Pago & Liberação de Vaga

<img src="caminho/para/sua/imagem4.png" width="800"/>

---

## ⚙️ Como Rodar o Projeto Localmente

### ✅ Pré-requisitos

- [.NET 6 ou superior](https://dotnet.microsoft.com/)
- [MySQL Server](https://www.mysql.com/)
- Visual Studio ou VS Code com extensão C#

### 🚀 Passos para Execução

1. **Clone o repositório:**
   ```bash
   git clone https://github.com/seu-usuario/seu-repositorio.git
2. Configure o appsettings.json com suas credenciais MySQL:
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=estacionamento;Uid=root;Pwd=senha;"
}
3.Crie o banco de dados e execute os scripts SQL (em /Scripts)
4. Execute a aplicação:
   dotnet watch run

Estrutura do Projeto

📁 Estacionamento/
├── Controllers/
├── Models/
├── Views/
├── wwwroot/
├── Program.cs
├── appsettings.json
└── README.md
