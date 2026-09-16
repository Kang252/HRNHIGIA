(function () {
    'use strict';
    var root = document.getElementById('hrmAi');
    if (!root) return;

    var launcher = document.getElementById('hrmAiLauncher');
    var panel = document.getElementById('hrmAiPanel');
    var closeButton = document.getElementById('hrmAiClose');
    var form = document.getElementById('hrmAiForm');
    var input = document.getElementById('hrmAiQuestion');
    var messages = document.getElementById('hrmAiMessages');
    var suggestions = document.getElementById('hrmAiSuggestions');
    var submitButton = form.querySelector('button[type="submit"]');

    function setOpen(open) {
        root.classList.toggle('is-open', open);
        panel.setAttribute('aria-hidden', open ? 'false' : 'true');
        launcher.setAttribute('aria-expanded', open ? 'true' : 'false');
        if (open) window.setTimeout(function () { input.focus(); }, 100);
    }

    function message(text, type, linkUrl, linkLabel) {
        var row = document.createElement('div');
        row.className = 'hrm-ai-message ' + (type === 'user' ? 'is-user' : type === 'error' ? 'is-error' : 'is-bot');
        var bubble = document.createElement('div');
        bubble.textContent = text;
        row.appendChild(bubble);
        if (linkUrl && linkLabel) {
            var link = document.createElement('a');
            link.href = linkUrl;
            link.textContent = linkLabel;
            link.className = 'hrm-ai-link';
            bubble.appendChild(link);
        }
        messages.appendChild(row);
        messages.scrollTop = messages.scrollHeight;
    }

    function renderSuggestions(items) {
        if (!Array.isArray(items) || !items.length) return;
        suggestions.innerHTML = '';
        items.slice(0, 5).forEach(function (text) {
            var button = document.createElement('button');
            button.type = 'button';
            button.textContent = text;
            suggestions.appendChild(button);
        });
    }

    async function ask(question) {
        message(question, 'user');
        submitButton.disabled = true;
        input.disabled = true;
        root.classList.add('is-loading');
        try {
            var token = form.querySelector('input[name="__RequestVerificationToken"]').value;
            var response = await fetch(root.dataset.endpoint, {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
                body: JSON.stringify({ Question: question })
            });
            var result = await response.json();
            if (!response.ok || !result.success) throw new Error(result.message || 'Không thể nhận câu trả lời.');
            message(result.data.Answer, 'bot', result.data.LinkUrl, result.data.LinkLabel);
            renderSuggestions(result.data.Suggestions);
        } catch (error) {
            message(error.message || 'Không thể kết nối trợ lý lúc này.', 'error');
        } finally {
            submitButton.disabled = false;
            input.disabled = false;
            root.classList.remove('is-loading');
            input.focus();
        }
    }

    launcher.addEventListener('click', function () { setOpen(!root.classList.contains('is-open')); });
    closeButton.addEventListener('click', function () { setOpen(false); });
    suggestions.addEventListener('click', function (event) {
        var button = event.target.closest('button');
        if (button) ask(button.textContent.trim());
    });
    form.addEventListener('submit', function (event) {
        event.preventDefault();
        var question = input.value.trim();
        if (!question) return;
        input.value = '';
        ask(question);
    });
    input.addEventListener('keydown', function (event) {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            form.requestSubmit();
        }
    });
    document.addEventListener('keydown', function (event) {
        if (event.key === 'Escape' && root.classList.contains('is-open')) setOpen(false);
    });
})();
