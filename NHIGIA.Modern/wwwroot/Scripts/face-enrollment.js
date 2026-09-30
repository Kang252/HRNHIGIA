(() => {
    'use strict';
    const root = document.getElementById('faceEnrollment');
    if (!root) return;
    const $ = id => document.getElementById(id), userId = Number(root.dataset.userId);
    const statusLabels = {PENDING: 'Chờ HR duyệt', ACTIVE: 'HR đã duyệt mẫu', REJECTED: 'HR từ chối mẫu', REVOKED: 'Đã thu hồi mẫu'};
    const statusDetails = {
        PENDING: 'Mẫu đang chờ HR đối chiếu. Bạn có thể thu hồi nếu cần chụp và gửi lại.',
        ACTIVE: 'Ảnh tham chiếu đã được HR xác nhận. Lượt chấm vào/ra vẫn chờ quản lý đối chiếu; so khớp tự động và liveness chưa được kích hoạt.',
        REJECTED: 'Kiểm tra ghi chú của HR rồi chụp ảnh mới để gửi lại.',
        REVOKED: 'Mẫu đã ngừng sử dụng. Chụp ảnh mới và gửi HR duyệt khi bạn muốn đăng ký lại.'
    };
    let enrollment = null, stateReady = false, busy = false, stream = null, photo = null, photoUrl = null, cameraEpoch = 0;
    let loading = null;
    const canRegister = () => stateReady && (!enrollment || ['REJECTED', 'REVOKED'].includes(enrollment.StatusCode));
    function message(text, error = false) {
        $('faceMessage').textContent = text;
        $('faceMessage').hidden = !text;
        $('faceMessage').classList.toggle('is-error', error);
    }
    function controls() {
        $('faceCaptureFields').disabled = busy || !canRegister();
        $('faceSubmit').disabled = busy || !photo || !$('faceConsent').checked || !canRegister();
        $('faceCapture').disabled = busy || !stream;
        $('faceRefresh').disabled = busy;
        $('faceRevoke').disabled = busy;
        root.querySelectorAll('#facePending button, #facePending textarea, #faceActive button, #faceActive textarea, #faceSearchForm button').forEach(element => { element.disabled = busy; });
    }
    const displayDate = value => {
        if (!value) return '';
        const date = new Date(value);
        return Number.isNaN(date.getTime()) ? '' : new Intl.DateTimeFormat('vi-VN', {
            timeZone: 'Asia/Ho_Chi_Minh', day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit'
        }).format(date);
    };
    function body(values = {}) {
        const data = new FormData();
        data.append('__RequestVerificationToken', $('faceForm').querySelector('[name=__RequestVerificationToken]').value);
        Object.entries(values).forEach(([key, value]) => data.append(key, String(value)));
        return data;
    }
    async function api(action, data) {
        const controller = new AbortController(), timeout = setTimeout(() => controller.abort(), 30000);
        try {
            const response = await fetch('/FaceEnrollment/' + action, {method: data ? 'POST' : 'GET', body: data,
                credentials: 'same-origin', cache: 'no-store', signal: controller.signal,
                headers: {Accept: 'application/json', 'X-Requested-With': 'XMLHttpRequest'}});
            const result = await response.json().catch(() => null);
            if (!response.ok || !result?.Success) {
                throw new Error(response.status === 401 || response.redirected
                    ? 'Phiên đăng nhập đã hết hạn. Đăng nhập lại rồi tải lại trang.'
                    : result?.Message || result?.message || 'Không xử lý được yêu cầu. Vui lòng tải lại hoặc thử lại.');
            }
            return result.Data;
        } catch (error) {
            if (error.name === 'AbortError') throw new Error('Kết nối mất quá lâu. Tải lại trạng thái để kiểm tra kết quả trước khi gửi lại.');
            if (error instanceof TypeError) throw new Error('Không có kết nối. Ảnh chưa gửi chỉ còn trong trang hiện tại; kết nối mạng rồi thử lại.');
            throw error;
        } finally { clearTimeout(timeout); }
    }
    function stopCamera() {
        cameraEpoch++;
        if (stream) stream.getTracks().forEach(track => track.stop());
        stream = null;
        $('faceVideo').srcObject = null;
        $('faceVideo').hidden = true;
        controls();
    }
    function clearPhoto() {
        photo = null;
        if (photoUrl) URL.revokeObjectURL(photoUrl);
        photoUrl = null;
        $('facePreview').hidden = true;
        $('facePreview').removeAttribute('src');
        $('faceConsent').checked = false;
        controls();
    }
    function render() {
        const status = enrollment?.StatusCode;
        $('faceStatus').textContent = statusLabels[status] || 'Chưa đăng ký';
        $('faceStatus').dataset.status = status || '';
        $('faceStatusDetail').textContent = statusDetails[status] || 'Chụp ảnh trực tiếp và gửi HR đối chiếu với hồ sơ của bạn.';
        $('faceDates').textContent = enrollment ? `Đã gửi: ${displayDate(enrollment.CreatedAt)}${enrollment.ReviewedAt ? ' · Xử lý: ' + displayDate(enrollment.ReviewedAt) : ''}` : '';
        $('faceReviewNote').textContent = enrollment?.ReviewNote ? 'Ghi chú của HR: ' + enrollment.ReviewNote : '';
        $('faceReviewNote').hidden = !enrollment?.ReviewNote;
        const showReference = !!enrollment && ['PENDING', 'ACTIVE'].includes(status);
        $('faceReference').hidden = !showReference;
        if (showReference) $('faceReference').src = '/FaceEnrollment/Photo?id=' + encodeURIComponent(enrollment.Id);
        else $('faceReference').removeAttribute('src');
        $('faceRevokePanel').hidden = !showReference;
        $('faceRegisterHint').textContent = canRegister()
            ? 'Ảnh mới sẽ chờ HR duyệt trước khi được dùng làm mẫu tham chiếu.'
            : 'Bạn đã có mẫu đang chờ duyệt hoặc đã được duyệt. Thu hồi mẫu hiện tại nếu cần đăng ký lại.';
        if (!canRegister()) { stopCamera(); clearPhoto(); }
        controls();
    }
    function element(tag, text, className) {
        const item = document.createElement(tag);
        if (text != null) item.textContent = text;
        if (className) item.className = className;
        return item;
    }
    function renderPending(data) {
        $('faceReviewPanel').hidden = !data.CanReview;
        $('facePending').replaceChildren();
        if (!data.CanReview) return;
        const pending = (data.Pending || []).filter(item => item.UserId !== userId && item.StatusCode === 'PENDING');
        $('facePendingCount').textContent = pending.length + ' mẫu chờ duyệt';
        if (!pending.length) { $('facePending').append(element('p', 'Không có mẫu chờ duyệt.', 'face-muted')); return; }
        pending.forEach(item => {
            const card = element('article', null, 'face-review-item');
            card.append(element('h3', item.DisplayName || 'Nhân viên'));
            card.append(element('p', 'Đã gửi: ' + displayDate(item.CreatedAt), 'face-muted'));
            const image = element('img');
            image.alt = 'Ảnh đăng ký của ' + (item.DisplayName || 'nhân viên');
            image.src = '/FaceEnrollment/Photo?id=' + encodeURIComponent(item.Id);
            image.loading = 'lazy';
            image.addEventListener('error', () => {
                image.hidden = true;
                card.append(element('p', 'Không tải được ảnh. Tải lại trước khi duyệt hoặc từ chối để nhân viên gửi ảnh mới.', 'face-note'));
                actions.querySelector('[data-approve]')?.remove();
            }, {once: true});
            card.append(image);
            const noteId = 'faceReviewNote-' + item.Id, label = element('label', 'Ghi chú đối chiếu / lý do từ chối (bắt buộc)');
            label.htmlFor = noteId;
            const note = element('textarea', null, 'hrm-control');
            note.id = noteId; note.rows = 2; note.maxLength = 1000; note.required = true;
            card.append(label, note);
            const actions = element('div', null, 'face-actions');
            [true, false].forEach(approve => {
                const button = element('button', approve ? 'Duyệt mẫu' : 'Từ chối', approve ? 'hrm-btn hrm-btn-primary' : 'hrm-btn');
                button.type = 'button';
                if (approve) button.dataset.approve = 'true';
                button.addEventListener('click', () => {
                    if (approve && (!image.complete || !image.naturalWidth)) { message('Chờ ảnh tải đầy đủ trước khi duyệt mẫu.', true); return; }
                    if (!note.value.trim()) { message('Nhập ghi chú đối chiếu hoặc lý do từ chối trước khi xử lý.', true); note.focus(); return; }
                    if (approve && !window.confirm('Bạn đã đối chiếu ảnh với đúng nhân viên và đồng ý duyệt mẫu này?')) return;
                    mutate('Review', body({id: item.Id, approve, note: note.value.trim()}), approve ? 'Đã duyệt ảnh tham chiếu.' : 'Đã từ chối mẫu và gửi ghi chú cho nhân viên.');
                });
                actions.append(button);
            });
            card.append(actions);
            $('facePending').append(card);
        });
    }
    function renderActive(data) {
        $('faceActivePanel').hidden = !data.CanReview;
        $('faceActive').replaceChildren();
        if (!data.CanReview) return;
        const active = (data.Active || []).filter(item => item.StatusCode === 'ACTIVE');
        $('faceActiveCount').textContent = active.length + ' mẫu';
        if (!active.length) { $('faceActive').append(element('p', 'Không có mẫu đang sử dụng phù hợp với tìm kiếm.', 'face-muted')); return; }
        active.forEach(item => {
            const card = element('article', null, 'face-review-item');
            card.append(element('h3', item.DisplayName || 'Nhân viên'));
            card.append(element('p', 'HR đã duyệt: ' + displayDate(item.ReviewedAt), 'face-muted'));
            const image = element('img');
            image.alt = 'Ảnh tham chiếu của ' + (item.DisplayName || 'nhân viên');
            image.src = '/FaceEnrollment/Photo?id=' + encodeURIComponent(item.Id);
            image.loading = 'lazy';
            image.addEventListener('error', () => { image.hidden = true; card.append(element('p', 'Ảnh mẫu không tải được hoặc đã được thu hồi.', 'face-note')); }, {once: true});
            card.append(image);
            const noteId = 'faceActiveNote-' + item.Id, label = element('label', 'Lý do thu hồi (bắt buộc)');
            label.htmlFor = noteId;
            const note = element('textarea', null, 'hrm-control');
            note.id = noteId; note.rows = 2; note.maxLength = 1000; note.required = true;
            card.append(label, note);
            const actions = element('div', null, 'face-actions'), revoke = element('button', 'Thu hồi mẫu', 'hrm-btn face-revoke');
            revoke.type = 'button';
            revoke.addEventListener('click', () => {
                if (!note.value.trim()) { message('Nhập lý do thu hồi mẫu trước khi tiếp tục.', true); note.focus(); return; }
                if (!window.confirm('Thu hồi và xóa ảnh mẫu của ' + (item.DisplayName || 'nhân viên') + '? Nhân viên sẽ cần đăng ký và được duyệt mẫu mới.')) return;
                mutate('Revoke', body({id: item.Id, note: note.value.trim()}), 'Đã thu hồi mẫu và xóa ảnh tham chiếu.');
            });
            actions.append(revoke); card.append(actions);
            $('faceActive').append(card);
        });
    }
    async function load() {
        if (loading) return loading;
        loading = (async () => {
            const search = $('faceSearch').value.trim();
            const data = await api('State' + (search ? '?search=' + encodeURIComponent(search) : ''));
            if (data.UserId != null && Number(data.UserId) !== userId) {
                stateReady = false; stopCamera(); clearPhoto();
                throw new Error('Tài khoản đã thay đổi. Tải lại trang trước khi đăng ký hoặc duyệt.');
            }
            enrollment = data.Enrollment;
            stateReady = true;
            render(); renderPending(data); renderActive(data);
        })();
        try { await loading; } finally { loading = null; controls(); }
    }
    async function mutate(action, data, successText) {
        if (busy) return;
        busy = true; controls(); message('Đang xử lý…');
        let saved = false;
        try {
            if (loading) await loading;
            await api(action, data);
            saved = true; stateReady = false;
            if (action === 'Submit' || action === 'Revoke') { stopCamera(); clearPhoto(); }
            message(successText);
            await load();
        } catch (error) { message(saved ? successText + ' Chưa tải lại được trạng thái: ' + error.message : error.message, true); }
        finally { busy = false; controls(); }
    }
    $('faceOpenCamera').addEventListener('click', async () => {
        if (busy || !canRegister()) return;
        stopCamera(); clearPhoto(); message('');
        if (!navigator.mediaDevices?.getUserMedia) { message('Trình duyệt chưa hỗ trợ camera. Mở HRM bằng HTTPS trên trình duyệt hỗ trợ camera.', true); return; }
        const epoch = cameraEpoch;
        $('faceOpenCamera').disabled = true;
        try {
            const media = await navigator.mediaDevices.getUserMedia({video: {facingMode: 'user', width: {ideal: 960}, height: {ideal: 960}}, audio: false});
            if (epoch !== cameraEpoch || document.hidden || !canRegister()) { media.getTracks().forEach(track => track.stop()); return; }
            stream = media;
            $('faceVideo').srcObject = media;
            $('faceVideo').hidden = false;
            await $('faceVideo').play();
            $('faceCameraHint').textContent = 'Giữ khuôn mặt chính diện và bấm Chụp ảnh khi đã rõ nét.';
            controls();
        } catch (error) {
            stopCamera();
            message(error.name === 'NotAllowedError' ? 'Camera chưa được cấp quyền. Cho phép camera trong trình duyệt rồi thử lại.' : 'Không mở được camera. Kiểm tra thiết bị hoặc đóng ứng dụng khác đang dùng camera.', true);
        } finally { $('faceOpenCamera').disabled = false; }
    });
    $('faceCapture').addEventListener('click', async () => {
        const video = $('faceVideo');
        if (busy || !stream || !video.videoWidth || !video.videoHeight) { message('Camera chưa sẵn sàng. Chờ hình ảnh xuất hiện rồi chụp lại.', true); return; }
        const epoch = cameraEpoch, scale = Math.min(1, 960 / Math.max(video.videoWidth, video.videoHeight));
        const canvas = document.createElement('canvas');
        canvas.width = Math.round(video.videoWidth * scale); canvas.height = Math.round(video.videoHeight * scale);
        canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height);
        const captured = await new Promise(resolve => canvas.toBlob(resolve, 'image/jpeg', .85));
        if (epoch !== cameraEpoch || !canRegister()) return;
        if (!captured || captured.size > 2 * 1024 * 1024) { message('Ảnh chưa hợp lệ hoặc quá lớn. Vui lòng chụp lại.', true); return; }
        stopCamera(); clearPhoto();
        photo = captured;
        photoUrl = URL.createObjectURL(captured);
        $('facePreview').src = photoUrl; $('facePreview').hidden = false;
        $('faceCameraHint').textContent = 'Kiểm tra ảnh trước khi gửi. Bạn có thể mở camera để chụp lại.';
        $('faceOpenCamera').textContent = 'Chụp lại';
        controls();
    });
    $('faceConsent').addEventListener('change', controls);
    $('faceForm').addEventListener('submit', event => {
        event.preventDefault();
        if (busy || !canRegister()) return;
        if (!photo || !$('faceConsent').checked) { message('Chụp ảnh và xác nhận đồng ý trước khi gửi.', true); return; }
        const data = body({consent: true}); data.append('photo', photo, 'face-registration.jpg');
        mutate('Submit', data, 'Đã gửi mẫu khuôn mặt. Vui lòng chờ HR đối chiếu và duyệt.');
    });
    $('faceRevoke').addEventListener('click', () => {
        if (busy || !enrollment) return;
        const note = $('faceRevokeNote').value.trim();
        if (!note) { message('Nhập lý do thu hồi mẫu trước khi tiếp tục.', true); $('faceRevokeNote').focus(); return; }
        if (!window.confirm('Thu hồi mẫu hiện tại? Bạn cần gửi và được duyệt mẫu mới nếu muốn đăng ký lại.')) return;
        mutate('Revoke', body({id: enrollment.Id, note}), 'Đã thu hồi mẫu. Bạn có thể đăng ký ảnh mới.');
    });
    $('faceRefresh').addEventListener('click', () => { message(''); load().catch(error => message(error.message, true)); });
    $('faceSearchForm').addEventListener('submit', event => { event.preventDefault(); if (!busy) { message(''); load().catch(error => message(error.message, true)); } });
    document.addEventListener('visibilitychange', () => { if (document.hidden) stopCamera(); });
    window.addEventListener('pagehide', () => { stopCamera(); clearPhoto(); });
    $('faceReference').addEventListener('error', () => { $('faceReference').hidden = true; message('Không tải được ảnh đã đăng ký. Tải lại trang để kiểm tra.', true); });
    load().catch(error => message(error.message, true));
})();
