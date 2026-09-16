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
    var submitIcon = submitButton.querySelector('.material-icons');
    var history = [];
    var savedMessages = [];
    var isAsking = false;
    var reduceMotion = window.matchMedia && window.matchMedia('(prefers-reduced-motion: reduce)').matches;
    var storageKey = 'hrmAiConversation:v2:' + (root.dataset.userId || 'anonymous');

    function setOpen(open) {
        root.classList.toggle('is-open', open);
        panel.setAttribute('aria-hidden', open ? 'false' : 'true');
        launcher.setAttribute('aria-expanded', open ? 'true' : 'false');
        if (open) window.setTimeout(function () { input.focus(); }, reduceMotion ? 0 : 220);
    }

    function scrollToLatest(smooth) {
        if (messages.scrollTo) {
            messages.scrollTo({ top: messages.scrollHeight, behavior: smooth && !reduceMotion ? 'smooth' : 'auto' });
        } else {
            messages.scrollTop = messages.scrollHeight;
        }
    }

    function message(text, type, linkUrl, linkLabel) {
        var row = document.createElement('div');
        row.className = 'hrm-ai-message is-entering ' + (type === 'user' ? 'is-user' : type === 'error' ? 'is-error' : 'is-bot');
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
        window.requestAnimationFrame(function () { row.classList.remove('is-entering'); });
        scrollToLatest(type !== 'user');
        return row;
    }

    function remember(role, text, linkUrl, linkLabel) {
        savedMessages.push({ role: role, text: text, linkUrl: linkUrl || '', linkLabel: linkLabel || '' });
        if (savedMessages.length > 40) savedMessages = savedMessages.slice(-40);
        history.push({ Role: role, Text: text });
        if (history.length > 8) history = history.slice(-8);
        try {
            window.sessionStorage.setItem(storageKey, JSON.stringify({ messages: savedMessages, history: history }));
        } catch (_) {
            // The assistant remains usable when browser storage is disabled.
        }
    }

    function restoreConversation() {
        try {
            var saved = JSON.parse(window.sessionStorage.getItem(storageKey) || '{}');
            if (!Array.isArray(saved.messages)) return;
            savedMessages = saved.messages.slice(-40).filter(function (item) {
                return item && (item.role === 'user' || item.role === 'assistant') && typeof item.text === 'string';
            });
            history = Array.isArray(saved.history) ? saved.history.slice(-8) : [];
            savedMessages.forEach(function (item) {
                message(item.text, item.role === 'user' ? 'user' : 'bot', item.linkUrl, item.linkLabel);
            });
        } catch (_) {
            savedMessages = [];
            history = [];
        }
    }

    function showTyping() {
        var row = document.createElement('div');
        row.className = 'hrm-ai-message is-bot is-typing is-entering';
        row.setAttribute('role', 'status');
        row.setAttribute('aria-label', 'Trợ lý đang phân tích dữ liệu');
        var bubble = document.createElement('div');
        var label = document.createElement('span');
        label.className = 'hrm-ai-typing-label';
        label.textContent = 'Đang phân tích dữ liệu';
        var dots = document.createElement('span');
        dots.className = 'hrm-ai-typing-dots';
        dots.setAttribute('aria-hidden', 'true');
        dots.innerHTML = '<i></i><i></i><i></i>';
        bubble.appendChild(label);
        bubble.appendChild(dots);
        row.appendChild(bubble);
        messages.appendChild(row);
        window.requestAnimationFrame(function () { row.classList.remove('is-entering'); });
        scrollToLatest(true);
        return row;
    }

    function setLoading(loading) {
        isAsking = loading;
        submitButton.disabled = loading;
        input.disabled = loading;
        root.classList.toggle('is-loading', loading);
        panel.setAttribute('aria-busy', loading ? 'true' : 'false');
        messages.setAttribute('aria-busy', loading ? 'true' : 'false');
        suggestions.setAttribute('aria-disabled', loading ? 'true' : 'false');
        Array.prototype.forEach.call(suggestions.querySelectorAll('button'), function (button) {
            button.disabled = loading;
        });
        if (submitIcon) submitIcon.textContent = loading ? 'hourglass_top' : 'send';
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
        if (isAsking || !question) return;

        var requestHistory = history.slice(-8);
        message(question, 'user');
        remember('user', question);
        setLoading(true);
        var typingMessage = showTyping();
        var loadingStartedAt = Date.now();

        try {
            var token = form.querySelector('input[name="__RequestVerificationToken"]').value;
            var response = await fetch(root.dataset.endpoint, {
                method: 'POST',
                credentials: 'same-origin',
                headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
                body: JSON.stringify({ Question: question, History: requestHistory })
            });
            var result = await response.json();
            var remainingDelay = Math.max(0, 450 - (Date.now() - loadingStartedAt));
            if (remainingDelay) await new Promise(function (resolve) { window.setTimeout(resolve, remainingDelay); });
            typingMessage.remove();
            if (!response.ok || !result.success) throw new Error(result.message || 'Không thể nhận câu trả lời.');
            message(result.data.Answer, 'bot', result.data.LinkUrl, result.data.LinkLabel);
            remember('assistant', result.data.Answer, result.data.LinkUrl, result.data.LinkLabel);
            root.dataset.provider = result.data.UsedGemini ? 'gemini' : 'database';
            renderSuggestions(result.data.Suggestions);
        } catch (error) {
            if (typingMessage.isConnected) typingMessage.remove();
            message(error.message || 'Không thể kết nối trợ lý lúc này.', 'error');
        } finally {
            setLoading(false);
            input.focus();
        }
    }

    launcher.addEventListener('click', function () { setOpen(!root.classList.contains('is-open')); });
    closeButton.addEventListener('click', function () { setOpen(false); });
    suggestions.addEventListener('click', function (event) {
        var button = event.target.closest('button');
        if (button && !button.disabled) ask(button.textContent.trim());
    });
    form.addEventListener('submit', function (event) {
        event.preventDefault();
        var question = input.value.trim();
        if (!question || isAsking) return;
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

    restoreConversation();
})();
