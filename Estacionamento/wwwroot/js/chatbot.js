(() => {
const messagesEl = document.getElementById("chatbot-messages");
    const inputEl = document.getElementById("chatbot-input-field"); 
    const sendBtn = document.getElementById("chatbot-send");
    const closeBtn = document.getElementById("chatbot-close");
    const toggleBtn = document.getElementById("chatbot-toggle");  

// Função para adicionar mensagens na tela
function appendMessage(sender, text) {
    const msg = document.createElement("div");
    msg.className = sender === "user" ? "message user" : "message bot"; // usa .message + .user/.bot
    msg.innerText = text;
    messagesEl.appendChild(msg);
    messagesEl.scrollTop = messagesEl.scrollHeight;
}

// Função para enviar mensagem ao backend
async function sendMessage(text = null) {
    const message = text || inputEl.value.trim();
    if (!message) return;

    appendMessage("user", message);
    inputEl.value = "";

    try {
        const response = await fetch("/api/chatbot/perguntar", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ pergunta: message })
        });

        const data = await response.json();
        appendMessage("bot", data.resposta);
    } catch (err) {
        appendMessage("bot", "❌ Erro ao conectar com o assistente.");
    }
}

// Eventos
if (sendBtn) {
    sendBtn.addEventListener("click", () => sendMessage());
}

if (inputEl) {
    inputEl.addEventListener("keypress", (e) => {
        if (e.key === "Enter") sendMessage();
    });
}

if (closeBtn) {
    closeBtn.addEventListener("click", () => {
        const container = document.getElementById("chatbot-container");
        if (container) container.style.display = "none";
    });
}

if (toggleBtn) {
    toggleBtn.addEventListener("click", () => {
        const container = document.getElementById("chatbot-container");
        if (container) {
            container.style.display =
                (container.style.display === "none" || container.style.display === "")
                ? "flex"
                : "none";
        }
    });
}})();
