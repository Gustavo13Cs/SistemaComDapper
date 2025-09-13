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

        // 1. Cria e mostra o indicador de "digitando..."
        const typingIndicator = createTypingIndicator();
        messages.appendChild(typingIndicator);
        messages.scrollTop = messages.scrollHeight;

        try {
            const response = await fetch("/chatbot/ask", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ message: text })
            });

            const data = await response.json();
            
            // 2. Remove o indicador antes de mostrar a resposta
            messages.removeChild(typingIndicator); 
            appendMessage("Bot", data.resposta || "Erro ao obter resposta.");

        } catch (err) {
            // 3. Remove o indicador MESMO SE DER ERRO
            messages.removeChild(typingIndicator); 
            appendMessage("Bot", "⚠️ Não foi possível conectar ao servidor.");
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

    function createTypingIndicator() {
        const div = document.createElement("div");
        div.classList.add("chat-message", "bot-typing");
        div.innerHTML = `<span></span><span></span><span></span>`;
        return div;
    }
    
});
