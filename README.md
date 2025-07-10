Sistema de Estacionamento com Dapper
Este é um Sistema de Estacionamento desenvolvido utilizando C# e Dapper para a manipulação de dados, com um banco de dados MySQL. O sistema gerencia as informações de clientes, veículos, vagas e tickets, atualizando os valores automaticamente com base no tempo de permanência no estacionamento e permitindo que os tickets sejam pagos e atualizados.

Funcionalidades
Gerenciamento de Clientes: Cadastrar e editar informações dos clientes, incluindo CPF e nome.

Gestão de Veículos: Registrar os veículos dos clientes, incluindo a placa, modelo e marca.

Tickets de Estacionamento: Gerenciar os tickets, atualizando automaticamente o valor conforme o tempo de permanência e permitindo o pagamento do ticket.

Vagas de Estacionamento: Exibir e liberar vagas conforme o movimento dos veículos no estacionamento.

Interface de Usuário: A interface é desenvolvida com HTML, CSS e JavaScript, proporcionando uma experiência intuitiva para o usuário.

Tecnologias Utilizadas
C#: Linguagem principal utilizada para a criação do sistema.

Dapper: Micro ORM para interação com o banco de dados MySQL.

MySQL: Banco de dados utilizado para armazenar as informações do sistema.

HTML/CSS/JavaScript: Tecnologias usadas para a construção da interface do usuário.

Banco de Dados
O banco de dados utilizado é o MySQL, que contém as seguintes tabelas principais:

Clientes: Contém os dados dos clientes (ID, nome, CPF).

Veículos: Registra as informações dos veículos dos clientes (ID, placa, modelo, marca).

Tickets: Registra os tickets de estacionamento (ID, cliente, veículo, local, data de entrada, data de saída, valor).

Vagas: Controla as vagas de estacionamento disponíveis e suas ocupações.

Fluxo de Funcionamento
Cadastro de Clientes e Veículos: O cliente pode ser cadastrado com seu nome e CPF, e um veículo pode ser vinculado ao cliente.

Abertura de Ticket: Um ticket é gerado quando o veículo entra no estacionamento, registrando a data de entrada, a vaga utilizada e o valor total.

Atualização de Tickets: O sistema atualiza o valor do ticket automaticamente com base no tempo de permanência do veículo no estacionamento.

Pagamento de Ticket: Quando o cliente efetua o pagamento, o ticket é atualizado com a data de saída e o valor pago, e a vaga é liberada para outro veículo.

Interface Intuitiva: A interface do sistema permite que o usuário consulte, edite e exclua informações de clientes, veículos e tickets.

Como Rodar o Projeto
Clone o repositório para o seu ambiente local:

bash
Copiar
Editar
git clone <URL_DO_REPOSITORIO>
Abra o projeto no Visual Studio ou na sua IDE preferida.

Configure o banco de dados MySQL com as credenciais corretas no arquivo de configuração do projeto.

Execute o projeto no Visual Studio. O sistema estará disponível no seu navegador.

Estrutura de Pastas
bash
Copiar
Editar
ESTACIONAMENTO/
│
├── Controllers/         # Controladores de interação com o frontend
├── DTO/                 # Objetos de transferência de dados
├── Models/              # Modelos de dados
├── Repositories/        # Repositórios para interação com o banco de dados
├── Views/               # Arquivos de visualização (HTML)
├── wwwroot/             # Arquivos estáticos (CSS, JS)
│   ├── css/             
│   ├── js/
│   ├── lib/
│   └── favicon.ico
├── appsettings.json     # Configurações de aplicação
├── Estacionamento.sln   # Arquivo da solução do Visual Studio
└── Program.cs           # Arquivo principal do projeto
Screenshots
Aqui estão alguns prints de tela do sistema:

Tela inicial do sistema:

Página de clientes:

Página de veículos:

Página de tickets:

Contribuição
Se você deseja contribuir para este projeto, siga os seguintes passos:

Fork o repositório.

Crie uma nova branch (git checkout -b feature-nova-funcionalidade).

Faça as modificações necessárias e adicione testes se for o caso.

Faça o commit das suas alterações (git commit -am 'Adiciona nova funcionalidade').

Faça o push para a branch (git push origin feature-nova-funcionalidade).

Crie um novo Pull Request.
