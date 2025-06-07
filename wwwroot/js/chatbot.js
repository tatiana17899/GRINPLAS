document.addEventListener("DOMContentLoaded", function () {
  const chatbotContainer = document.getElementById("chatbot-container");
  const chatbotCircle = document.getElementById("chatbot-circle");
  const closeBtn = document.getElementById("close-chatbot");
  const userInput = document.getElementById("user-input");
  const sendBtn = document.getElementById("send-btn");
  const chatMessages = document.getElementById("chatbot-messages");

  // Configuración del chatbot
  const botConfig = {
    name: "Asistente de Reclamos",
    greetings: [
      "¡Hola! Soy tu asistente virtual. ¿En qué puedo ayudarte hoy?",
      "¡Buen día! Estoy aquí para ayudarte con tus reclamos. ¿Cómo puedo asistirte?",
      "Hola, soy el asistente de reclamos. ¿Qué necesitas?",
    ],
    fallbackResponses: [
      "No entiendo su solicitud. ¿Puede ser más específico?",
      "No entiendo su solicitud. ¿Puede ser más específico?",
      "No entiendo su solicitud. ¿Puede ser más específico?",
    ],
    claimResponses: [
      "Lamento escuchar eso, Entiendo que necesitas hacer un reclamo. Para agilizar el proceso, puedes usar nuestro formulario dedicado.",
      "Lamento escuchar que tienes un problema. Para asistirte mejor con tu reclamo, te recomiendo usar nuestro sistema especializado.",
      "¡Oh vaya, Lamento mucho escuchar eso!Para gestionar tu reclamo de manera eficiente, tenemos un proceso establecido. Permíteme guiarte.",
    ],
    thanksResponses: [
      "¡Gracias a ti por contactarnos! ¿Hay algo más en lo que pueda ayudarte?",
      "De nada, es un placer ayudarte. No dudes en preguntar si necesitas algo más.",
      "¡Gracias! Recuerda que estoy aquí para lo que necesites.",
    ],
    farewellResponses: [
      "¡Hasta luego! Si necesitas más ayuda, aquí estaré.",
      "Fue un placer ayudarte. ¡Que tengas un buen día!",
      "Nos vemos. No dudes en volver si tienes más preguntas.",
    ],
    patterns: [
      {
        regex: /(hola|buen(a|o)s (días|tardes|noches)|saludos|hi|hello)/i,
        responses: "greetings",
      },
      {
        regex: /(gracias|agradezco|te lo agradezco|thanks|thank you)/i,
        responses: "thanksResponses",
      },
      {
        regex: /(adios|chao|hasta luego|nos vemos|bye)/i,
        responses: "farewellResponses",
      },
      {
        regex:
          /(reclamo|queja|problema|error|incidente|falla|mal funcionamiento|insatisfecho|reclamar)/i,
        responses: "claimResponses",
      },
      {
        regex:
          /(quiero|necesito|deseo|me gustaría|how to|cómo|como|formulario|proceso|hacer|realizar)/i,
        responses: [
          "Puedo ayudarte con eso. ¿Se trata de un reclamo o consulta general?",
          "Entiendo tu necesidad. ¿Podrías especificar si es un reclamo o otra consulta?",
          "Para asistirte mejor, ¿podrías confirmar si esto está relacionado con un reclamo?",
        ],
      },
    ],
  };

  // Funciones de utilidad
  function getRandomResponse(responseType) {
    const responses = Array.isArray(responseType)
      ? responseType
      : botConfig[responseType];
    return responses[Math.floor(Math.random() * responses.length)];
  }

  function detectIntent(message) {
    const lowerMsg = message.toLowerCase();

    // Detección especial para reclamos
    if (
      lowerMsg.includes("reclamo") ||
      lowerMsg.includes("queja") ||
      lowerMsg.includes("problema") ||
      lowerMsg.includes("error")
    ) {
      return {
        intent: "claim",
        response: getRandomResponse(botConfig.claimResponses),
      };
    }

    // Buscar coincidencias con patrones
    for (const pattern of botConfig.patterns) {
      if (pattern.regex.test(message)) {
        return {
          intent: pattern.regex.source,
          response: Array.isArray(pattern.responses)
            ? getRandomResponse(pattern.responses)
            : getRandomResponse(botConfig[pattern.responses]),
        };
      }
    }

    // Respuesta por defecto
    return {
      intent: "unknown",
      response: getRandomResponse(botConfig.fallbackResponses),
    };
  }

  // Funciones del chat
  function addMessage(sender, message, isHtml = false) {
    const messageElement = document.createElement("div");
    messageElement.style.margin = "5px 0";

    if (sender === "user") {
      messageElement.style.textAlign = "right";
      messageElement.innerHTML = `
        <span style="background: #1B665E; color: white; padding: 8px 12px; 
        border-radius: 18px; display: inline-block; max-width: 80%; word-wrap: break-word;">
          ${message}
        </span>
      `;
    } else {
      messageElement.style.textAlign = "left";
      if (isHtml) {
        messageElement.innerHTML = message;
      } else {
        messageElement.innerHTML = `
          <span style="background: #f1f1f1; padding: 8px 12px; border-radius: 18px; 
          display: inline-block; max-width: 80%; word-wrap: break-word;">
            <strong>${botConfig.name}:</strong> ${message}
          </span>
        `;
      }
    }

    chatMessages.appendChild(messageElement);
    chatMessages.scrollTop = chatMessages.scrollHeight;
  }

  function addReclamoButton() {
    const buttonHtml = `
      <div style="text-align: center; margin: 10px 0;">
        <button onclick="window.location.href='/Reclamaciones/Cliente'" 
          style="background: #1B665E; color: white; border: none; padding: 10px 20px; 
          border-radius: 20px; cursor: pointer; font-weight: 500;">
          Ir al Formulario de Reclamos
        </button>
        <p style="font-size: 12px; margin-top: 5px; color: #666;">Serás redirigido a nuestro formulario oficial</p>
      </div>
    `;
    addMessage("bot", buttonHtml, true);
  }

  function processUserInput() {
    const message = userInput.value.trim();
    if (message) {
      addMessage("user", message);
      userInput.value = "";

      // Mostrar "escribiendo..."
      const typingIndicator = document.createElement("div");
      typingIndicator.id = "typing-indicator";
      typingIndicator.style.textAlign = "left";
      typingIndicator.style.margin = "5px 0";
      typingIndicator.style.fontStyle = "italic";
      typingIndicator.style.color = "#666";
      typingIndicator.innerHTML = `<span>${botConfig.name} está escribiendo...</span>`;
      chatMessages.appendChild(typingIndicator);
      chatMessages.scrollTop = chatMessages.scrollHeight;

      // Simular tiempo de respuesta
      setTimeout(() => {
        document.getElementById("typing-indicator")?.remove();

        const { intent, response } = detectIntent(message);

        addMessage("bot", response);

        // Si es un reclamo, mostrar el botón
        if (intent === "claim") {
          addReclamoButton();
        }
      }, 1000 + Math.random() * 1500);
    }
  }

  // Event listeners
  chatbotCircle.addEventListener("click", function () {
    chatbotContainer.style.display = "block";
    chatbotCircle.style.display = "none";
    if (chatMessages.children.length === 0) {
      addMessage("bot", getRandomResponse(botConfig.greetings));
    }
  });

  closeBtn.addEventListener("click", function () {
    chatbotContainer.style.display = "none";
    chatbotCircle.style.display = "flex";
  });

  sendBtn.addEventListener("click", processUserInput);
  userInput.addEventListener("keypress", function (e) {
    if (e.key === "Enter") {
      processUserInput();
    }
  });
});
