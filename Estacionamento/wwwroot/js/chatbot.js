const toggleBot = () => {
        const box = document.getElementById('chatBotBox');
        box.style.display = box.style.display === 'none' ? 'block' : 'none';
    };

    document.getElementById('botToggle').addEventListener('click', toggleBot);

    const enviarPergunta = () => {
        const input = document.getElementById('userInput');
        const pergunta = input.value;
        const chatLog = document.getElementById('chatLog');

        // Mostra pergunta do usuário
        chatLog.innerHTML += `<div class="text-end"><span class="badge bg-primary mb-2">${pergunta}</span></div>`;

        // Chama backend (a gente cria o endpoint na próxima etapa)
        fetch('/chatbot/responder?pergunta=' + encodeURIComponent(pergunta))
            .then(res => res.text())
            .then(resposta => {
                chatLog.innerHTML += `<div class="text-start"><span class="badge bg-secondary mb-2">${resposta}</span></div>`;
                chatLog.scrollTop = chatLog.scrollHeight;
            });

        input.value = '';
        return false;
    };

    function enviarAtalho(pergunta) {
    const chatLog = document.getElementById('chatLog');
    
    chatLog.innerHTML += `<div class="text-end"><span class="badge bg-primary mb-2">${pergunta}</span></div>`;

    fetch('/chatbot/responder?pergunta=' + encodeURIComponent(pergunta))
        .then(res => res.text())
        .then(resposta => {
            chatLog.innerHTML += `<div class="text-start"><span class="badge bg-secondary mb-2">${resposta}</span></div>`;
            chatLog.scrollTop = chatLog.scrollHeight;
        });
}
