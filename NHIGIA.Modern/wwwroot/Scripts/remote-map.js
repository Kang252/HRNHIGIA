(() => {
    'use strict';
    const config=document.getElementById('remoteMapConfig');
    const valid=(lat,lng)=>lat!==null && lng!==null && lat!=='' && lng!=='' && Number.isFinite(Number(lat)) && Number.isFinite(Number(lng)) && Math.abs(Number(lat))<=90 && Math.abs(Number(lng))<=180;
    function create(canvas,status,editable,onSelect) {
        if (!window.L) { status.textContent='Không tải được bản đồ. Bạn vẫn có thể nhập tọa độ hoặc lấy GPS.'; return null; }
        const instance=L.map(canvas,{scrollWheelZoom:true}).setView([10.7769,106.7009],12);
        let marker,circle,layer;
        const attribution='&copy; <a href="https://www.openstreetmap.org/copyright" target="_blank" rel="noopener">OpenStreetMap</a> contributors';
        const extra=document.createElement('span');extra.textContent=config?.dataset.attribution || '';
        const tileUrl=config?.dataset.tileUrl || '';
        // Load only when visible. Browser cache and origin referrer are preserved; no prefetch/offline tiles.
        const observer=new IntersectionObserver(entries=>{
            if (!entries.some(e=>e.isIntersecting)) return;
            instance.invalidateSize();
            if (layer) return;
            if (!tileUrl.startsWith('https://')) {status.textContent='Chưa cấu hình máy chủ bản đồ HTTPS hợp lệ.';return;}
            layer=L.tileLayer(tileUrl,{maxZoom:19,attribution:attribution+(extra.textContent?' · '+extra.innerHTML:''),referrerPolicy:'strict-origin-when-cross-origin',keepBuffer:0}).addTo(instance);
            layer.on('tileerror',()=>{status.textContent='Chưa tải được nền bản đồ. Kiểm tra kết nối; tọa độ và bán kính vẫn được giữ.';});
            layer.on('tileload',()=>{status.textContent=editable?'Bấm trên bản đồ hoặc kéo ghim để chọn địa điểm.':'Vị trí đã ghi nhận; bản đồ chỉ để xem.';});
        });observer.observe(canvas);
        const resize=new ResizeObserver(()=>{if(canvas.clientWidth && canvas.clientHeight) instance.invalidateSize({pan:false});});resize.observe(canvas);
        if(editable) instance.on('click',e=>onSelect(e.latlng.lat,e.latlng.wrap().lng));
        function update(lat,lng,radius,center=false) {
            if (!valid(lat,lng)) {
                if(marker){marker.remove();marker=null;}if(circle){circle.remove();circle=null;}
                delete canvas.dataset.latitude;delete canvas.dataset.longitude;delete canvas.dataset.radius;
                status.textContent='Chưa chọn vị trí. Bấm trên bản đồ, nhập tọa độ hoặc lấy vị trí hiện tại.';
                return;
            }
            const point=[Number(lat),Number(lng)],meters=Math.max(0,Math.min(100000,Number(radius)||0));
            if (!marker) {
                marker=L.marker(point,{draggable:editable,icon:L.divIcon({className:'remote-map-pin',iconSize:[22,22],iconAnchor:[11,11]}),title:editable?'Kéo để chọn vị trí':'Vị trí ghi nhận'}).addTo(instance);
                if(editable) marker.on('dragend',()=>{const p=marker.getLatLng().wrap();onSelect(p.lat,p.lng);});
                center=true;
            } else marker.setLatLng(point);
            if(circle) circle.remove();
            circle=meters?L.circle(point,{radius:meters,color:'#1769e0',weight:2,fillOpacity:.12}).addTo(instance):null;
            if(center) {instance.invalidateSize();if(circle) instance.fitBounds(circle.getBounds(),{padding:[25,25],maxZoom:17,animate:false});else instance.setView(point,16,{animate:false});}
            canvas.dataset.latitude=String(lat);canvas.dataset.longitude=String(lng);canvas.dataset.radius=String(meters);
        }
        return {update,destroy(){observer.disconnect();resize.disconnect();instance.remove();}};
    }
    let picker;
    function syncPicker(center=false) {
        const canvas=document.getElementById('planMapCanvas'), status=document.getElementById('planMapStatus');
        if(!canvas || !canvas.clientWidth) return;
        picker ||= create(canvas,status,true,(lat,lng)=>{
            document.getElementById('planLat').value=lat.toFixed(6);
            document.getElementById('planLng').value=lng.toFixed(6);
            document.getElementById('planLat').dispatchEvent(new Event('input',{bubbles:true}));
        });
        picker?.update(document.getElementById('planLat').value,document.getElementById('planLng').value,document.getElementById('planRadius').value,center);
    }
    document.addEventListener('remote:mapChanged',()=>syncPicker(true));
    document.addEventListener('remote:formOpened',()=>requestAnimationFrame(()=>syncPicker(true)));
    document.getElementById('remoteLocation')?.addEventListener('toggle',()=>syncPicker(true));
    let dialog,viewer;
    function show(lat,lng,radius=0,label='Vị trí chấm công') {
        if(!valid(lat,lng)) return;
        if(!dialog) {
            dialog=document.createElement('dialog');dialog.className='remote-map-dialog';dialog.setAttribute('aria-labelledby','remoteMapTitle');
            dialog.innerHTML='<header><h2 id="remoteMapTitle"></h2><button type="button" class="hrm-btn" aria-label="Đóng bản đồ">Đóng</button></header><div class="remote-map-canvas" id="remoteViewMap"></div><p class="remote-map-status" role="status"></p><p class="remote-map-caption"></p><a target="_blank" rel="noopener noreferrer">Mở bằng Google Maps</a>';
            document.body.appendChild(dialog);
            dialog.querySelector('button').onclick=()=>dialog.close();
            dialog.addEventListener('close',()=>{viewer?.destroy();viewer=null;});
        }
        dialog.querySelector('h2').textContent=label;
        dialog.querySelector('.remote-map-caption').textContent=`Tọa độ: ${Number(lat).toFixed(6)}, ${Number(lng).toFixed(6)}${Number(radius)>0?' · Vòng tròn: '+Math.round(radius)+' m':''}`;
        dialog.querySelector('a').href=`https://www.google.com/maps?q=${Number(lat)},${Number(lng)}`;
        if(!dialog.open) dialog.showModal();
        viewer?.destroy();viewer=create(dialog.querySelector('.remote-map-canvas'),dialog.querySelector('.remote-map-status'),false);
        viewer?.update(lat,lng,radius,true);
    }
    window.HrmRemoteMap={show,syncPicker};
    document.addEventListener('click',event=>{
        const button=event.target.closest('[data-remote-map]');
        if(button) show(button.dataset.lat,button.dataset.lng,button.dataset.radius,button.dataset.mapTitle);
    });
})();
