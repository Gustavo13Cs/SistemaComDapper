# 🚗 Sistema de Estacionamento


Bem-vindo ao **Sistema de Estacionamento**, uma aplicação moderna e responsiva desenvolvida em **C#** com **Dapper** e **MySQL**, que permite o gerenciamento completo de um estacionamento com atualização em tempo real de vagas e tickets.

<div align="center">
  <img src="https://i.ibb.co/W4d77Nyj/Projeto.png" />
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

<img src="https://i.ibb.co/W4d77Nyj/Projeto.png" width="800"/>

### 👥 Gerenciamento de Clientes

<img src="https://i.ibb.co/fdh39rZc/PROJETO4.png" width="800"/>

### 💳 Ticket Pago & Liberação de Vaga

<img src="https://i.ibb.co/d4hhV5Nw/PROJETO5.png" width="800"/>

---

## ⚙️ Como Rodar o Projeto Localmente

### ✅ Pré-requisitos

- [.NET 6 ou superior](https://dotnet.microsoft.com/)
- [MySQL Server](https://www.mysql.com/)
- Visual Studio ou VS Code com extensão C#

### 🚀 Passos para Execução

1. **Clone o repositório:**
   ```bash
   git clone https://github.com/SistemaComDapper/tree/main/Estacionamento
   
2. **Configure o appsettings.json com suas credenciais MySQL:**
   ```bash
   "ConnectionStrings": {"DefaultConnection": "Server=localhost;Database=estacionamento;Uid=root;Pwd=senha;"}

3. **Crie o banco de dados e execute os scripts SQL (em /Scripts)**
4. **Execute a aplicação:**
   ```bash
   dotnet watch run

