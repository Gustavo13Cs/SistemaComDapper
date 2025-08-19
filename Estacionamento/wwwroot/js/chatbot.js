document.addEventListener("DOMContentLoaded", function () {
    const chatbotToggle = document.getElementById("chatbot-toggle");
    const chatbotWindow = document.getElementById("chatbot-window");
    const chatbotMessages = document.getElementById("chatbot-messages");
    const chatbotInput = document.getElementById("chatbot-input");
    const chatbotSend = document.getElementById("chatbot-send");
    const quickButtons = document.getElementById("chatbot-quick-buttons");

    chatbotToggle.addEventListener("click", function () {
        chatbotWindow.style.display = chatbotWindow.style.display === "none" ? "flex" : "none";
    });

    chatbotSend.addEventListener("click", sendMessage);
    chatbotInput.addEventListener("keypress", function (e) {
        if (e.key === "Enter") sendMessage();
    });

    function sendMessage(text = null) {
        const message = text || chatbotInput.value.trim();
        if (message === "") return;

        addMessage(message, "user");
        chatbotInput.value = "";

        fetch("/api/chatbot/perguntar", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ texto: message })
        })
            .then(res => res.json())
            .then(data => {
                addMessage(data.resposta, "bot");
            })
            .catch(err => {
                addMessage("Erro ao conectar com o bot 😢", "bot");
                console.error(err);
            });
    }

    function addMessage(text, sender) {
        const div = document.createElement("div");
        div.classList.add("chatbot-message", sender);
        div.textContent = text;
        chatbotMessages.appendChild(div);
        chatbotMessages.scrollTop = chatbotMessages.scrollHeight;
    }

    // 🔘 Botões rápidos
    const perguntasRapidas = [
        "Tickets ativos",
        "Clientes em atraso",
        "Horário de pico",
        "Tempo médio",
        "Receita do mês",
        "Histórico vaga"
    ];

    perguntasRapidas.forEach(p => {
    const btn = document.createElement("button");
    btn.textContent = p;
    btn.onclick = () => sendMessage(p);
    quickButtons.appendChild(btn);
});
});
