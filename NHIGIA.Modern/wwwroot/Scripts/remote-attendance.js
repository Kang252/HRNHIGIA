(() => {
    'use strict';
    const root = document.getElementById('remoteAttendance');
    if (!root) return;
    const plansOnly = root.dataset.mode === 'plans';
    const userId = Number(root.dataset.userId), $ = id => document.getElementById(id);
    const esc = v => String(v ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
    const labels = {PENDING:'Chờ xác minh / duyệt',APPROVED:'Đã chấp nhận',REJECTED:'Đã từ chối',CANCELLED:'Đã hủy',IN:'Vào làm',OUT:'Kết thúc làm',VISIT:'Ghé khách hàng'};
    const localDate = (d = new Date()) => new Intl.DateTimeFormat('en-CA', {timeZone:'Asia/Ho_Chi_Minh',year:'numeric',month:'2-digit',day:'2-digit'}).format(d);
    const day = s => String(s || '').slice(0,10), time = s => String(s || '').slice(0,5);
    const formatTime = s => String(s || '').replace('T',' ').slice(0,19);
    const map = (lat,lng,radius=0,title='Vị trí chấm công') => lat != null && lng != null ? `<button type="button" class="hrm-btn" data-remote-map data-lat="${Number(lat)}" data-lng="${Number(lng)}" data-radius="${Number(radius)||0}" data-map-title="${esc(title)}">Xem bản đồ</button>` : '';
    let plans = [], capture = null, stream = null, syncing = false, faceStatus = null, openCvEnabled = false;
    const faceMatchLabels = {MATCH:'Đạt ngưỡng so khớp',NO_MATCH:'Chưa đạt ngưỡng so khớp',NO_FACE:'Không tìm thấy khuôn mặt',MULTIPLE_FACES:'Ảnh có nhiều khuôn mặt',LOW_QUALITY:'Ảnh chưa đủ chất lượng',INVALID_IMAGE:'Ảnh không hợp lệ',UNAVAILABLE:'Dịch vụ thử nghiệm chưa sẵn sàng',DISABLED:'Thử nghiệm chưa bật',BUSY:'Máy chủ đang bận',TIMEOUT:'So khớp quá thời gian'};
    function faceTrialState() {
        if (plansOnly) return;
        const allowed = openCvEnabled && $('remoteKind').value !== 'VISIT';
        $('remoteFaceTrial').hidden = !allowed;
        $('remoteFaceConsent').disabled = !allowed || !capture;
        if (!allowed) $('remoteFaceConsent').checked = false;
        $('remoteFaceTrialUnavailable').hidden = openCvEnabled;
        $('remoteFaceTrialUnavailable').textContent = 'Thử so khớp OpenCV chưa bật. Lượt chấm vẫn được gửi để xác minh thủ công.';
    }
    function faceMatchResult(p) {
        if (!p.FaceEnrollmentId) return '';
        const score = typeof p.FaceMatchScore === 'number' && Number.isFinite(p.FaceMatchScore) && Math.abs(p.FaceMatchScore) <= 1 ? p.FaceMatchScore.toFixed(3) : null;
        const threshold = typeof p.FaceMatchThreshold === 'number' && Number.isFinite(p.FaceMatchThreshold) ? p.FaceMatchThreshold.toFixed(3) : null;
        const result = p.FaceMatchStatus ? `<div class="remote-face-result"><strong>OpenCV · ${esc(faceMatchLabels[p.FaceMatchStatus] || 'Chưa có kết quả xác định')}</strong>${score !== null ? `<p>Điểm tương đồng cosine: <b>${score}</b>${threshold !== null ? ` · Ngưỡng: ${threshold}` : ''}. Đây không phải tỷ lệ xác thực danh tính.</p>` : ''}${p.FaceMatchModelVersion ? `<p class="hrm-field-note">Mô hình: ${esc(p.FaceMatchModelVersion)}</p>` : ''}<p>Chưa kiểm tra người thật (liveness). Cần đối chiếu thủ công để quyết định công.</p></div>` : '';
        const action = openCvEnabled && p.CanCompareFace ? `<button type="button" class="hrm-btn" data-compare-face="${p.Id}" data-face-owner="${p.UserId === userId}">${p.FaceMatchStatus ? 'Thử lại so khớp OpenCV' : 'Thử so khớp OpenCV'}</button>` : '';
        return result + (action ? `<div class="remote-actions remote-face-trial-actions">${action}</div>` : '');
    }
    function ensureFaceReady() {
        if ($('remoteKind').value !== 'VISIT' && faceStatus !== 'ACTIVE')
            throw new Error('Cần đăng ký mẫu khuôn mặt và được HR duyệt trước khi chấm vào/ra. Mở “Khuôn mặt chấm công” để đăng ký.');
    }
    function message(text, error = false) {
        const target=$('remotePlanPanel')?.closest('#tcCreateModal.is-open') ? $('remotePlanMessage') : $('remoteMessage');
        target.textContent=text; target.hidden=!text; target.classList.toggle('is-error',error);
    }
    function token() { return ($('remotePlanForm') || $('remotePunchForm')).querySelector('[name=__RequestVerificationToken]').value; }
    async function api(action, body) {
        const controller = new AbortController(), timer = setTimeout(() => controller.abort(),30000);
        try {
            const response = await fetch('/RemoteAttendance/' + action, {method:body?'POST':'GET', body, credentials:'same-origin', signal:controller.signal, headers:{Accept:'application/json','X-Requested-With':'XMLHttpRequest'}});
            const data = await response.json().catch(() => null);
            if (!response.ok || !data?.Success) {
                const error = new Error(response.status === 401 ? 'Phiên đăng nhập hết hạn. Đăng nhập lại đúng tài khoản rồi mở lại trang để đồng bộ.' : data?.Message || data?.message || 'Không thể xử lý yêu cầu. Vui lòng thử lại.');
                error.status = response.status; throw error;
            }
            return data.Data;
        } finally { clearTimeout(timer); }
    }
    function formBody(values) { const body = new FormData(); Object.entries(values).forEach(([k,v]) => { if (v != null && v !== '') body.append(k,String(v)); }); body.append('__RequestVerificationToken',token()); return body; }
    function stopCamera() { if (stream) stream.getTracks().forEach(t => t.stop()); stream=null; $('remoteVideo').srcObject=null; $('remoteVideo').hidden=true; $('remoteCapture').disabled=true; }
    function resetPhoto() { capture=null; $('remoteFaceConsent').checked=false; faceTrialState(); $('remotePhoto').hidden=true; $('remotePhoto').removeAttribute('src'); $('remoteGps').textContent='GPS sẽ được lấy cùng lúc chụp ảnh.'; stopCamera(); }
    const locate = () => new Promise((resolve,reject) => {
        if (!navigator.geolocation) return reject(new Error('Thiết bị không hỗ trợ GPS.'));
        navigator.geolocation.getCurrentPosition(p => resolve(p.coords), () => reject(new Error('Chưa lấy được GPS. Kiểm tra quyền vị trí hoặc ghi rõ lý do để quản lý xác minh.')), {enableHighAccuracy:true,timeout:15000,maximumAge:0});
    });

    // User-specific, encrypted queue. The non-extractable key remains in this browser's IndexedDB.
    const database = plansOnly ? null : new Promise((resolve,reject) => {
        const request = indexedDB.open('nhigia-remote-' + userId,1);
        request.onupgradeneeded = () => { request.result.createObjectStore('queue',{keyPath:'id'}); request.result.createObjectStore('meta'); };
        request.onsuccess = () => resolve(request.result); request.onerror = () => reject(new Error('Không mở được bộ nhớ offline của trình duyệt.'));
    });
    async function dbOp(store,method,...args) {
        const db = await database;
        return new Promise((resolve,reject) => {
            const tx = db.transaction(store,['get','getAll'].includes(method)?'readonly':'readwrite');
            const req = tx.objectStore(store)[method](...args);
            tx.oncomplete=()=>resolve(req.result); tx.onerror=()=>reject(tx.error || req.error); tx.onabort=()=>reject(tx.error || new Error('Không lưu được bộ nhớ offline.'));
        });
    }
    let keyPromise;
    function key() {
        return keyPromise ||= (async () => {
            let saved=await dbOp('meta','get','key'); if(saved) return saved;
            const generated=await crypto.subtle.generateKey({name:'AES-GCM',length:256},false,['encrypt','decrypt']);
            try { await dbOp('meta','add',generated,'key'); return generated; }
            catch { saved=await dbOp('meta','get','key'); if(saved) return saved; throw new Error('Không tạo được khóa bộ nhớ offline.'); }
        })();
    }
    async function saveQueued(item) {
        const iv=crypto.getRandomValues(new Uint8Array(12));
        const bytes=await crypto.subtle.encrypt({name:'AES-GCM',iv},await key(),new TextEncoder().encode(JSON.stringify(item)));
        await dbOp('queue','put',{id:item.ClientId,iv,bytes});
    }
    async function queued() {
        const encrypted=await dbOp('queue','getAll'), result=[];
        for(const row of encrypted) {
            try { result.push(JSON.parse(new TextDecoder().decode(await crypto.subtle.decrypt({name:'AES-GCM',iv:row.iv},await key(),row.bytes)))); }
            catch { throw new Error('Có bản ghi offline không đọc được. Không xóa dữ liệu trình duyệt; liên hệ IT.'); }
        }
        return result.sort((a,b)=>a.CapturedAt.localeCompare(b.CapturedAt));
    }
    async function showQueue() {
        const items=await queued();
        $('remoteQueue').innerHTML=items.length?items.map(x=>`<article class="remote-item"><strong>${esc(labels[x.Kind])} · ${esc(x.PlaceName)}</strong><p>${esc(new Date(x.CapturedAt).toLocaleString('vi-VN'))}</p><p>${esc(x.Error || 'Đã lưu trên thiết bị, chưa ghi vào bảng công.')}</p><div class="remote-actions"><button type="button" class="hrm-btn" data-queue-note="${esc(x.ClientId)}">Bổ sung giải trình</button><button type="button" class="hrm-btn" data-discard="${esc(x.ClientId)}">Xóa bản chờ</button></div></article>`).join(''):'Không có bản ghi chờ đồng bộ.';
    }
    async function sync() {
        if(syncing || !navigator.onLine) return;
        syncing=true; $('remoteSync').disabled=true;
        try {
            const work=async()=>{
                let count=0;
                for(const item of await queued()) {
                    try {
                        const {Photo,Error,...values}=item;
                        const body=formBody(values);
                        const raw=atob(Photo.split(',')[1]), bytes=Uint8Array.from(raw,c=>c.charCodeAt(0));
                        body.append('photo',new Blob([bytes],{type:'image/jpeg'}),'capture.jpg');
                        await api('Punch',body);
                        await dbOp('queue','delete',item.ClientId); count++;
                    } catch(error) {
                        item.Error=error.message;
                        if(!error.status) item.WasOffline=true;
                        await saveQueued(item);
                        if(!error.status || error.status>=500 || error.status===401 || error.status===403) break;
                    }
                }
                if(count) { message(`Đã đồng bộ ${count} lượt. Kiểm tra trạng thái bên dưới; lượt chờ xác minh chưa được tính công.`); await load(); }
            };
            if(navigator.locks) await navigator.locks.request('remote-sync-'+userId,work); else await work();
        } catch(error) { message(error.message,true); }
        finally { syncing=false; $('remoteSync').disabled=false; await showQueue().catch(e=>message(e.message,true)); }
    }
    function planHint() {
        const plan=plans.find(x=>x.Plan.Id===Number($('remotePlan').value))?.Plan;
        $('remotePlanHint').textContent=plan?`${labels[plan.StatusCode]} · ${day(plan.FromDate)} → ${day(plan.ToDate)} · ${time(plan.WindowStart)}–${time(plan.WindowEnd)}${plan.IsFlexible?' · linh hoạt '+plan.RequiredMinutes+' phút/ngày':''}`:'Đăng ký lịch trước khi chấm công.';
        if(plan && !$('remotePlace').value) $('remotePlace').value=plan.PlaceName;
        const visit=$('remoteKind').querySelector('[value=VISIT]'); visit.disabled=plan?.Mode==='HOME';
        if(visit.disabled && $('remoteKind').value==='VISIT') $('remoteKind').value='IN';
    }
    async function load() {
        const data=await api(`State?from=${encodeURIComponent($('remoteFrom').value)}&to=${encodeURIComponent($('remoteTo').value)}`);
        if(data.UserId!==userId) throw new Error('Tài khoản đã thay đổi. Tải lại trang trước khi tiếp tục.');
        plans=data.Plans;
        openCvEnabled = data.OpenCvEnabled === true;
        if (!plansOnly) {
        faceStatus = data.FaceEnrollmentStatus;
        $('remoteFaceState').textContent = faceStatus === 'ACTIVE'
            ? 'Mẫu đã được HR duyệt · lượt vào/ra chờ xác minh thủ công.'
            : faceStatus === 'PENDING' ? 'Mẫu khuôn mặt đang chờ HR duyệt.' : 'Chưa có mẫu khuôn mặt được duyệt để chấm vào/ra.';
        const previous=$('remotePlan').value;
        $('remotePlan').innerHTML='<option value="">Chọn lịch của bạn</option>'+plans.filter(x=>x.Plan.UserId===userId && ['APPROVED','PENDING'].includes(x.Plan.StatusCode)).map(({Plan:p})=>`<option value="${p.Id}">${esc(p.PlaceName)} · ${day(p.FromDate)} · ${esc(labels[p.StatusCode])}</option>`).join('');
        if([...$('remotePlan').options].some(o=>o.value===previous)) $('remotePlan').value=previous;
        planHint();
        faceTrialState();
        }
        if ($('planTrip')) {
        const oldTrip=$('planTrip').value;
        $('planTrip').innerHTML='<option value="">Không liên kết</option>'+data.Trips.map(p=>`<option value="${p.Id}">${esc(p.RequestCode)} · ${day(p.StartDate)} → ${day(p.EndDate)}</option>`).join(''); $('planTrip').value=oldTrip;
        }
        $('remotePlans').innerHTML=plans.length?plans.map(({Plan:p,CanReview,CanCancel})=>`<article class="remote-item"><header><strong>${esc(p.DisplayName)} · ${p.Mode==='HOME'?'Làm tại nhà':'Đi thị trường'} · #${p.Id}</strong><span class="hrm-badge">${esc(labels[p.StatusCode])}</span></header><p>${esc(p.PlaceName)} · ${day(p.FromDate)} → ${day(p.ToDate)} · ${time(p.WindowStart)}–${time(p.WindowEnd)}</p><p>${p.IsFlexible?'Linh hoạt '+p.RequiredMinutes+' phút/ngày':'Ca theo khung giờ'} · Nghỉ ${p.BreakMinutes} phút · ${p.Latitude!=null?'Bán kính '+p.RadiusMeters+' m':'Đi theo tuyến, ghi GPS thực tế'}</p><p>${esc(p.Note)} ${p.ReviewNote?' / Xử lý: '+esc(p.ReviewNote):''}</p><div class="remote-actions">${map(p.Latitude,p.Longitude,p.RadiusMeters,'Vùng làm việc đã đăng ký')}${p.UserId===userId?`<button type="button" class="hrm-btn" data-copy-plan="${p.Id}">Dùng lại thông tin</button>`:''}${CanReview?`<button class="hrm-btn" data-review="ReviewPlan" data-id="${p.Id}" data-approve="true">Duyệt lịch</button><button class="hrm-btn" data-review="ReviewPlan" data-id="${p.Id}" data-approve="false">Từ chối</button>`:''}${CanCancel?`<button class="hrm-btn" data-cancel-plan="${p.Id}">Hủy lịch</button>`:''}</div></article>`).join(''):'Chưa có đăng ký trong khoảng này.';
        if ($('remotePunches')) $('remotePunches').innerHTML=data.Punches.length?data.Punches.map(p=>`<article class="remote-item"><header><strong>${esc(p.DisplayName)} · ${esc(labels[p.Kind])}</strong><span class="hrm-badge">${esc(labels[p.StatusCode])}</span></header><p>${formatTime(p.CheckTime)} · ${esc(p.PlaceName)} · Lịch #${p.PlanId}</p><p>GPS: ${p.AccuracyMeters!=null?'sai số ±'+Math.round(p.AccuracyMeters)+' m':'chưa có'}${p.DistanceMeters!=null?' · Cách điểm đăng ký '+Math.round(p.DistanceMeters)+' m':''}</p>${p.ReviewReason?`<p class="remote-note">${esc(p.ReviewReason)}</p>`:''}<p>${esc(p.Note)} ${p.ReviewNote?' / Xử lý: '+esc(p.ReviewNote):''}</p>${p.FaceEnrollmentId?`<p>Danh tính: ${p.FaceVerificationStatus==='MANUAL_APPROVED'?'Đã đối chiếu thủ công':p.FaceVerificationStatus==='MANUAL_REJECTED'?'Đã từ chối khi xác minh':'Chờ đối chiếu thủ công'}</p>${p.CanReview?`<details class="remote-face-compare"><summary>Đối chiếu ảnh mẫu và ảnh chấm công</summary><div><figure><img src="/RemoteAttendance/FaceReference/${p.Id}" alt="Ảnh mẫu đã được HR duyệt; không còn xem được nếu đã thu hồi" loading="lazy"><figcaption>Mẫu đã được HR duyệt</figcaption></figure><figure><img src="/RemoteAttendance/Photo/${p.Id}" alt="Ảnh của lượt chấm đang duyệt" loading="lazy"><figcaption>Ảnh lượt chấm công</figcaption></figure></div><p>Chỉ chấp nhận khi xác minh đúng nhân viên. Nếu mẫu đã bị thu hồi, yêu cầu đăng ký và chấm lại.</p></details>`:''}`:''}${faceMatchResult(p)}<div class="remote-actions">${map(p.Latitude,p.Longitude,p.AccuracyMeters,'Vị trí chấm công · vòng sai số GPS')}<a class="hrm-btn" href="/RemoteAttendance/Photo/${p.Id}" target="_blank" rel="noopener">Xem ảnh</a>${p.CanReview?`<button class="hrm-btn" data-review="ReviewPunch" data-id="${p.Id}" data-approve="true">Chấp nhận lượt</button><button class="hrm-btn" data-review="ReviewPunch" data-id="${p.Id}" data-approve="false">Từ chối</button>`:''}</div></article>`).join(''):'Chưa có lượt chấm trong khoảng này.';
    }

    if (!plansOnly) {
    $('remoteOpenCamera').onclick=async()=>{
        stopCamera(); $('remoteOpenCamera').disabled=true;
        try { ensureFaceReady(); stream=await navigator.mediaDevices.getUserMedia({audio:false,video:{facingMode:$('remoteKind').value==='VISIT'?'environment':'user',width:{ideal:960},height:{ideal:720}}}); $('remoteVideo').srcObject=stream; $('remoteVideo').hidden=false; await $('remoteVideo').play(); $('remoteCapture').disabled=false; }
        catch (error) { message(faceStatus !== 'ACTIVE' && $('remoteKind').value !== 'VISIT' ? error.message : 'Không mở được camera. Hãy cấp quyền camera và dùng kết nối HTTPS.',true); }
        finally { $('remoteOpenCamera').disabled=false; }
    };
    $('remoteCapture').onclick=async()=>{
        $('remoteCapture').disabled=true;
        $('remoteFaceConsent').checked=false;
        try {
            const video=$('remoteVideo'); if(!video.videoWidth) throw new Error('Camera chưa sẵn sàng.');
            const canvas=document.createElement('canvas'), scale=Math.min(1,960/video.videoWidth);
            canvas.width=video.videoWidth*scale; canvas.height=video.videoHeight*scale;
            canvas.getContext('2d').drawImage(video,0,0,canvas.width,canvas.height);
            capture={Photo:canvas.toDataURL('image/jpeg',0.78),CapturedAt:new Date().toISOString()};
            faceTrialState();
            $('remotePhoto').src=capture.Photo; $('remotePhoto').hidden=false; stopCamera();
            $('remoteGps').textContent='Đang lấy GPS…'; $('remoteSend').disabled=true;
            try { const gps=await locate(); Object.assign(capture,{Latitude:gps.latitude,Longitude:gps.longitude,AccuracyMeters:gps.accuracy}); $('remoteGps').textContent=`Đã lấy vị trí; sai số ±${Math.round(gps.accuracy)} m.`; }
            catch(error) { $('remoteGps').textContent=error.message; }
        } catch(error) { message(error.message,true); }
        finally { $('remoteSend').disabled=false; }
    };
    $('remoteKind').onchange=resetPhoto; $('remotePlan').onchange=()=>{ resetPhoto(); planHint(); };
    $('remotePunchForm').onsubmit=async event=>{
        event.preventDefault(); $('remoteSend').disabled=true;
        try {
            ensureFaceReady();
            if(!capture) throw new Error('Chụp ảnh trước khi ghi nhận.');
            if(Date.now()-new Date(capture.CapturedAt).getTime()>5*60000) throw new Error('Ảnh đã quá 5 phút. Vui lòng chụp lại.');
            if(capture.Latitude==null && !$('remoteNote').value.trim()) throw new Error('Vui lòng giải trình khi không lấy được GPS.');
            if((await queued()).length>=50) throw new Error('Đã có 50 lượt chờ. Đồng bộ hoặc xử lý các lượt cũ trước.');
            const item={...capture,ClientId:crypto.randomUUID(),OwnerUserId:userId,PlanId:Number($('remotePlan').value),Kind:$('remoteKind').value,PlaceName:$('remotePlace').value.trim(),Note:$('remoteNote').value.trim() || (!navigator.onLine?'Ghi nhận trong lúc mất mạng.':''),WasOffline:!navigator.onLine,FaceMatchConsent:openCvEnabled && $('remoteKind').value !== 'VISIT' && $('remoteFaceConsent').checked};
            await saveQueued(item); resetPhoto(); $('remoteNote').value='';
            message('Đã lưu lượt chấm trên thiết bị. Đang chờ đồng bộ.'); await showQueue(); await sync();
        } catch(error) { message(error.message,true); }
        finally { $('remoteSend').disabled=false; }
    };
    }
    function planMode() { const home=$('planMode').value==='HOME'; $('planLat').required=home; $('planLng').required=home; $('remoteLocation').open=home; $('planTrip').disabled=home; if(home) $('planTrip').value=''; }
    function planMap() { const lat=$('planLat').value,lng=$('planLng').value; $('planMap').hidden=!lat||!lng; if(lat&&lng) $('planMap').href=`https://www.google.com/maps?q=${Number(lat)},${Number(lng)}`; document.dispatchEvent(new Event('remote:mapChanged')); }
    if ($('remotePlanForm')) {
    $('planMode').onchange=planMode;
    $('planLat').oninput=planMap; $('planLng').oninput=planMap; $('planRadius').oninput=planMap;
    $('planLocate').onclick=async()=>{ $('planLocate').disabled=true; try {const gps=await locate(); $('planLat').value=gps.latitude; $('planLng').value=gps.longitude; planMap(); message(`Đã lấy tọa độ đăng ký, sai số ±${Math.round(gps.accuracy)} m. Kiểm tra bản đồ trước khi gửi.`);}catch(e){message(e.message,true);}finally{$('planLocate').disabled=false;} };
    $('remotePlanForm').onsubmit=async event=>{
        event.preventDefault(); const button=event.target.querySelector('[type=submit]'); button.disabled=true;
        try { const body=new FormData(event.target); body.set('IsFlexible',$('planFlexible').checked); body.set('WorkDaysMask',[...$('remotePlanForm').querySelectorAll('.remote-day:checked')].reduce((sum,x)=>sum+Number(x.value),0)); body.set('__RequestVerificationToken',token()); await api('Plan',body); document.dispatchEvent(new Event('remote:planCreated')); message('Đã gửi đăng ký, quản lý sẽ nhận thông báo.'); await load(); }
        catch(error) { message(error.message,true); } finally { button.disabled=false; }
    };
    }
    root.addEventListener('click',async event=>{
        const button=event.target.closest('button');
        if(!button || !button.matches('[data-review],[data-cancel-plan],[data-copy-plan],[data-discard],[data-queue-note],[data-compare-face]')) return;
        try {
            if(button.dataset.compareFace) {
                const owner = button.dataset.faceOwner === 'true';
                const explanation = owner
                    ? 'Anh/chị đồng ý dùng ảnh lượt chấm đã lưu và mẫu đã được HR duyệt để thử so khớp trên máy chủ OpenCV do công ty vận hành? Ảnh không gửi đến AWS. Đây chưa phải kiểm tra người thật (liveness); lượt chấm vẫn chờ xác minh thủ công.'
                    : 'Thử so khớp ảnh lượt chấm và mẫu đã được HR duyệt trên máy chủ OpenCV do công ty vận hành? Nhân viên đã đồng ý cho lượt này. Kết quả không kiểm tra người thật (liveness) và không tự duyệt công.';
                if (!confirm(explanation)) return;
                button.disabled=true;
                await api('CompareFace',formBody({id:button.dataset.compareFace,consent:owner}));
                message('Đã xử lý yêu cầu thử so khớp. Xem kết quả của lượt chấm bên dưới; trạng thái công vẫn cần xác minh thủ công.');
                await load();
            } else if(button.dataset.review) {
                const note=prompt('Ghi chú xác minh / lý do xử lý (bắt buộc):'); if(!note?.trim()) return;
                button.disabled=true; await api(button.dataset.review,formBody({id:button.dataset.id,approve:button.dataset.approve,note})); message('Đã lưu kết quả xử lý.'); await load();
            } else if(button.dataset.cancelPlan) {
                if(!confirm('Hủy lịch này? Các lượt chấm đã xác minh vẫn được giữ.')) return;
                button.disabled=true; await api('CancelPlan',formBody({id:button.dataset.cancelPlan})); await load();
            } else if(button.dataset.copyPlan) {
                if (!$('remotePlanForm')) { location.href='/Home/LeaveRequests?remotePlan='+encodeURIComponent(button.dataset.copyPlan)+'#remoteRegistration'; return; }
                const plan=plans.find(x=>x.Plan.Id===Number(button.dataset.copyPlan))?.Plan; if(!plan) return;
                for(const input of $('remotePlanForm').elements) if(input.name && !['FromDate','ToDate','__RequestVerificationToken'].includes(input.name)) input.value=plan[input.name] ?? '';
                $('planFlexible').checked=plan.IsFlexible; $('remotePlanForm').querySelectorAll('.remote-day').forEach(x=>x.checked=(plan.WorkDaysMask&Number(x.value))!==0);
                $('planStart').value=time(plan.WindowStart); $('planEnd').value=time(plan.WindowEnd); planMode(); planMap(); window.openRemoteWorkRequest?.();
            } else if(button.dataset.discard) {
                if(!confirm('Xóa bản chờ chưa đồng bộ trên thiết bị?')) return;
                await dbOp('queue','delete',button.dataset.discard); await showQueue();
            } else if(button.dataset.queueNote) {
                const item=(await queued()).find(x=>x.ClientId===button.dataset.queueNote), note=prompt('Bổ sung giải trình:',item?.Note||'');
                if(item && note?.trim()) { item.Note=note.trim(); item.Error='Đã bổ sung ghi chú. Bấm Đồng bộ lại.'; await saveQueued(item); await showQueue(); }
            }
        } catch(error) { message(error.message,true); } finally { button.disabled=false; }
    });
    function connection() { $('remoteConnection').textContent=navigator.onLine?'Có kết nối mạng':'Đang offline'; }
    $('remoteRefresh').onclick=()=>load().catch(e=>message(e.message,true));
    if (!plansOnly) {
    $('remoteSync').onclick=sync;
    window.addEventListener('online',()=>{connection();sync();}); window.addEventListener('offline',connection);
    document.addEventListener('visibilitychange',()=>{if(document.hidden) stopCamera();}); window.addEventListener('pagehide',stopCamera);
    connection();
    }
    if ($('remotePlanForm')) { $('planFrom').value=$('planTo').value=localDate(); planMode(); }
    $('remoteFrom').value=localDate(new Date(Date.now()-7*86400000)); $('remoteTo').value=localDate(new Date(Date.now()+90*86400000));
    load().then(()=> {
        if (!plansOnly) return sync();
        const copyId=Number(new URLSearchParams(location.search).get('remotePlan'));
        if (copyId) root.querySelector(`[data-copy-plan="${copyId}"]`)?.click();
        else if (location.hash === '#remoteRegistration') window.openRemoteWorkRequest?.();
    }).catch(e=>message(e.message,true));
    if (plansOnly) window.addEventListener('hashchange',()=>{if(location.hash==='#remoteRegistration') window.openRemoteWorkRequest?.();});
    if (!plansOnly) showQueue().catch(e=>message(e.message,true));
})();
