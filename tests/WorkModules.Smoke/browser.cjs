// Run WorkModules.Smoke with --snapshots first. These pages use fixtures only.
const { chromium } = require(process.env.HRM_TEST_PLAYWRIGHT || 'playwright');
const fs = require('node:fs');
const path = require('node:path');
const http = require('node:http');
const assert = require('node:assert/strict');
const root = path.resolve(__dirname, '../..');
const snapshotDir = path.join(root, '.tmp-audit-ui');
const staticRoot = path.join(root, 'NHIGIA.Modern/wwwroot');
const mime = {'.css':'text/css','.js':'text/javascript','.png':'image/png','.woff2':'font/woff2','.woff':'font/woff','.ttf':'font/ttf','.svg':'image/svg+xml'};
const server = http.createServer((req,res) => {
    const url = new URL(req.url,'http://localhost');
    const filename = path.resolve(staticRoot,'.'+decodeURIComponent(url.pathname));
    if (!filename.startsWith(staticRoot+path.sep) || !fs.existsSync(filename) || fs.statSync(filename).isDirectory()) { res.writeHead(404).end(); return; }
    res.setHeader('Content-Type',mime[path.extname(filename)]||'application/octet-stream');
    res.end(fs.readFileSync(filename));
});
function snapshot(route,role='ADMIN') {
    if (route==='/asset-save-failed') return fs.readFileSync(path.join(snapshotDir,'asset-save-failed.html'),'utf8');
    return fs.readFileSync(path.join(snapshotDir,role+'-'+Buffer.from(route).toString('hex').toUpperCase()+'.html'),'utf8');
}
(async () => {
    await new Promise(resolve=>server.listen(0,'127.0.0.1',resolve));
    const origin='http://127.0.0.1:'+server.address().port;
    const browser=await chromium.launch({headless:true,channel:process.env.HRM_TEST_BROWSER || 'msedge'});
    const errors=[];
    try {
        const context=await browser.newContext();
        const page=await context.newPage();
        const requests=[];
        page.on('pageerror',error=>errors.push(error.message));
        await page.route('**/*',route=>{
            const u=new URL(route.request().url());
            if (u.origin!==origin) return route.abort();
            if (u.pathname==='/Work/NotificationCount') return route.fulfill({json:{Count:0}});
            if (u.pathname.startsWith('/Hrm/')) {
                requests.push({path:u.pathname,method:route.request().method(),body:route.request().postData()});
                const users=[{Id:1,DisplayName:'Local smoke',Username:'local-smoke',EmployeeCode:'NV001',DepartmentId:2,RoleCode:'ADMIN'},
                    {Id:20,DisplayName:'Nguyễn "An"',Username:'an',EmployeeCode:'NV020',DepartmentId:2,DepartmentName:'IT',RoleCode:'EMPLOYEE'}];
                const leave={Id:42,UserId:20,RequestCode:'NP42',DisplayName:'Nguyễn "An"',DepartmentName:'IT',DepartmentId:2,RoleCode:'MANAGER',
                    LeaveType:'Nghỉ phép',StartDate:'2026-09-25',EndDate:'2026-09-25',StatusCode:'PENDING_MANAGER',SessionCode:'Buổi sáng',
                    Reason:'Thử "nội dung" <script>không thực thi</script>',HasAttachment:true,AttachmentName:'Ghi chú.pdf'};
                const data=u.pathname==='/Hrm/Users'?users:u.pathname==='/Hrm/LeaveRequests'?[leave]:u.pathname==='/Hrm/ApprovalInbox'?
                    [{Source:'work',Kind:'vehicle',Id:99,Code:'XE99',RequestType:'Đặt xe',EmployeeName:'Nguyễn An',Title:'Công tác',Description:'Cần xe 4 chỗ'}]:[];
                return route.fulfill({json:{Success:true,Data:data}});
            }
            return route.continue();
        });
        async function open(route,role='ADMIN',disableStorage=false) {
            await page.goto(origin+'/Content/hrm-navigation.css'); // Establish a loopback origin for storage.
            if (disableStorage) await page.evaluate(()=>Object.defineProperty(window,'localStorage',{get(){throw new Error('Storage disabled');},configurable:true}));
            await page.setContent(snapshot(route,role).replace('<head>','<head><base href="'+origin+'/">'),{waitUntil:'networkidle'});
        }
        for (const width of [320,390,768,1024,1440]) {
            await page.setViewportSize({width,height:900});
            await open('/');
            const toggle=page.locator('.hrm-menu-toggle');
            if (width<1280) {
                assert.equal(await toggle.getAttribute('aria-expanded'),'false');
                await toggle.click();
                assert.equal(await toggle.getAttribute('aria-expanded'),'true');
                await page.keyboard.press('Escape');
                assert.equal(await toggle.getAttribute('aria-expanded'),'false');
                await toggle.click();
            }
            const section=page.locator('[data-menu-section="people"] button');
            const previous=await section.getAttribute('aria-expanded');
            await section.click();
            assert.notEqual(await section.getAttribute('aria-expanded'),previous);
            await section.click();
            const bounds=await page.evaluate(()=>{
                const menu=document.querySelector('.hrm-menu-toggle').getBoundingClientRect();
                const logo=document.querySelector('.navbar-brand').getBoundingClientRect();
                const logout=document.querySelector('.hrm-logout').getBoundingClientRect();
                return {left:menu.left,gap:logo.left-menu.right,right:logout.right,content:document.querySelector('section.content').getBoundingClientRect().width};
            });
            assert.ok(bounds.left>=0,'Menu button must remain fully inside viewport');
            assert.ok(bounds.gap>=6,'Menu/logo targets must not overlap');
            assert.ok(bounds.right<=width+1,'Header must fit viewport');
            if(width===1024) assert.ok(bounds.content>900,'Tablet main content must not be compressed by sidebar');
            console.log('PASS navigation '+width+'px');
        }
        await page.setViewportSize({width:1440,height:1000});
        // Storage denial used to prevent all click handlers from being installed.
        await open('/','ADMIN',true);
        var menuBefore=await page.locator('.hrm-menu-toggle').getAttribute('aria-expanded');
        await page.locator('.hrm-menu-toggle').click();
        assert.notEqual(await page.locator('.hrm-menu-toggle').getAttribute('aria-expanded'),menuBefore);
        console.log('PASS navigation with unavailable localStorage');
        await open('/Home/Approvals','MANAGER');
        assert.ok(await page.locator('#tcTableRows').innerText());
        assert.equal(await page.locator('#approvalInboxRows tr').count(),1);
        await page.locator('[data-inbox-index="0"][data-decision="approve"]').click();
        await page.waitForTimeout(100);
        assert.ok(requests.some(x=>x.path==='/Hrm/DecideApproval' && x.body.includes('Id=99')));
        await open('/Home/LeaveRequests','EMPLOYEE');
        await page.locator('#btnOpenCreateRequest').click();
        await page.waitForTimeout(200);
        console.log('PASS procedure modal opens');
        await page.locator('.tc-create-item[data-type="Công tác/Ra ngoài"]').click();
        await page.locator('#dynLocation').fill('Văn phòng chi nhánh');
        await page.locator('#dynReason').fill('Họp triển khai dự án');
        await page.locator('#dynAdvanceAmount').fill('-1000');
        page.once('dialog',dialog=>dialog.accept());
        var createsBefore=requests.filter(x=>x.path==='/Hrm/CreateLeave').length;
        await page.locator('#btnSubmitDynamicRequest').click();
        assert.equal(requests.filter(x=>x.path==='/Hrm/CreateLeave').length,createsBefore);
        await page.locator('#dynAdvanceAmount').fill('500000');
        await page.locator('#dynAttachmentFile').setInputFiles({name:'ghi-chu.pdf',mimeType:'application/pdf',buffer:Buffer.from('%PDF-1.4 test fixture')});
        await page.locator('#btnSubmitDynamicRequest').click();
        await page.waitForTimeout(100);
        const created=requests.filter(x=>x.path==='/Hrm/CreateLeave').at(-1);
        assert.ok(created && created.body.includes('ghi-chu.pdf') && created.body.includes('500.000'),'Procedure submits advance and attachment');
        console.log('PASS negative advance blocked; multipart attachment and advance submitted');
        await open('/asset-save-failed');
        await page.locator('#modalCreateAssetItem').waitFor({state:'visible'});
        assert.equal(await page.locator('#astName').inputValue(),'Laptop audit retained');
        assert.equal(await page.locator('#astPrice').inputValue(),'-10');
        assert.equal(await page.locator('#astDesc').inputValue(),'Keep my draft');
        assert.equal(await page.locator('#astRef').evaluate(field=>field.readOnly),true);
        assert.ok((await page.locator('#modalCreateAssetItem [role="alert"]').innerText()).includes('Nguyên giá tài sản không được âm'));
        await page.locator('#modalCreateAssetItem .modal-footer [data-dismiss="modal"]').click();
        await page.locator('#modalCreateAssetItem').waitFor({state:'hidden'});
        console.log('PASS asset editor restores failed draft and closes while offline');
        for(const route of ['/Work?kind=kpi','/Work?kind=payroll','/Work?kind=recruitment','/Work?kind=training','/Work?kind=overtime','/Work?kind=resignation','/Work?kind=transfer','/Work?kind=meeting','/Work?kind=vehicle','/Home/InternalCommunications','/Home/HanetIntegration']) {
            await open(route);
            const enabled=await page.locator('section.content form[method="post"] button[type="submit"]:enabled').count();
            if(route.startsWith('/Work?')) assert.equal(enabled,0,'Offline mutation form enabled: '+route);
            console.log('PASS rendered scripts '+route);
        }
        await page.setViewportSize({width:390,height:844});
        await open('/Home/Approvals','MANAGER');
        const mobileLayout=await page.evaluate(()=>({width:document.documentElement.scrollWidth,search:document.querySelector('.tc-search-box').getBoundingClientRect().height}));
        assert.ok(mobileLayout.width<=390,'Closed drawers must not cause horizontal page overflow');
        assert.ok(mobileLayout.search<60,'Mobile search must not expand to a tall column');
        await page.screenshot({path:path.join(snapshotDir,'approvals-mobile.png'),fullPage:true});
        if(errors.length) throw new Error([...new Set(errors)].join('\n'));
        console.log('PASS browser checks, no uncaught JavaScript errors');
    } finally { await browser.close(); server.close(); }
})().catch(error=>{console.error(error);server.close();process.exitCode=1;});
