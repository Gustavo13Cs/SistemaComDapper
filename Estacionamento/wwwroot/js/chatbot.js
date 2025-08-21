document.addEventListener("DOMContentLoaded", function () {
    const chatbotBody = document.getElementById("chatbot-body");
    const toggleBtn = document.getElementById("chatbot-toggle");
    const closeBtn = document.getElementById("chatbot-close"); 
    const sendBtn = document.getElementById("chat-send");
    const input = document.getElementById("chat-input");
    const messages = document.getElementById("chat-messages");

    // começa fechado
    chatbotBody.classList.add("hidden");

    toggleBtn.addEventListener("click", () => {
        chatbotBody.classList.remove("hidden"); 
        toggleBtn.style.display = "none";       
    });

    if (closeBtn) {
        closeBtn.addEventListener("click", () => {
            chatbotBody.classList.add("hidden");  
            setTimeout(() => {
                toggleBtn.style.display = "block"; 
            }, 300); 
        });
    }

    sendBtn.addEventListener("click", sendMessage);
    input.addEventListener("keypress", function (e) {
        if (e.key === "Enter") sendMessage();
    });

    async function sendMessage() {
        const text = input.value.trim();
        if (!text) return;

        appendMessage("Você", text);
        input.value = "";

        try {
            const response = await fetch("/chatbot/ask", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ message: text })
            });

            const data = await response.json();
            appendMessage("Bot", data.resposta || "Erro ao obter resposta.");
        } catch (err) {
            appendMessage("Bot", "⚠️ Não foi possível conectar ao servidor Ollama.");
        }
    }

    function appendMessage(sender, text) {
        const div = document.createElement("div");
        div.classList.add("chat-message");

        if (sender === "Você") {
            div.classList.add("user-message");
        } else {
            div.classList.add("bot-message");
        }

        div.innerHTML = `<span>${text}</span>`;
        messages.appendChild(div);
        messages.scrollTop = messages.scrollHeight;
    }
});
