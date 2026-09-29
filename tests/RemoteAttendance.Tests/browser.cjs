const {chromium}=require(process.env.HRM_TEST_PLAYWRIGHT || 'playwright');
const assert=require('node:assert/strict');
const path=require('node:path');
(async()=>{
    const origin=process.env.HRM_REMOTE_TEST_URL;
    assert(/^http:\/\/127\.0\.0\.1:\d+$/.test(origin),'only isolated loopback host allowed');
    const browser=await chromium.launch({headless:true,channel:'msedge',args:['--use-fake-ui-for-media-stream','--use-fake-device-for-media-stream']});
    try{
        const context=await browser.newContext({viewport:{width:390,height:844},permissions:['camera','geolocation'],geolocation:{latitude:10.77,longitude:106.69,accuracy:10}});
        await context.addCookies([{name:'NHIGIA.Auth.v3',value:process.env.HRM_REMOTE_TEST_COOKIE,url:origin}]);
        const page=await context.newPage(),errors=[];
        page.on('pageerror',e=>errors.push(e.message));
        await page.goto(origin+'/RemoteAttendance');
        await page.waitForFunction(()=>document.querySelector('#remotePlan').options.length>1);
        const plan=await page.locator('#remotePlan option').nth(1).getAttribute('value');
        await page.selectOption('#remotePlan',plan);
        await page.fill('#remotePlace','Khách hàng trình duyệt thử');
        await page.fill('#remoteNote','Lượt kiểm thử offline có ảnh và GPS.');
        await page.click('#remoteOpenCamera');
        await page.waitForFunction(()=>!document.querySelector('#remoteCapture').disabled);
        await page.click('#remoteCapture');
        await page.waitForFunction(()=>document.querySelector('#remoteGps').textContent.includes('Đã lấy vị trí'));
        await context.setOffline(true);
        await page.click('#remoteSend');
        await page.waitForFunction(()=>document.querySelectorAll('[data-queue-note]').length===1);
        assert((await page.locator('#remoteConnection').textContent()).includes('offline'));
        // No page-wide horizontal overflow on phone; review cards and forms stay inside the viewport.
        assert(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth+1));
        await page.evaluate(()=>scrollTo(0,0));
        await page.screenshot({path:path.join(process.env.HRM_REMOTE_TEST_OUTPUT,'remote-mobile.png'),fullPage:true});
        await context.setOffline(false);
        await page.waitForFunction(()=>document.querySelector('#remoteQueue').textContent.includes('Không có bản ghi'),{},{timeout:30000}).catch(async error=>{console.error('Queue:',await page.locator('#remoteQueue').innerText());console.error('Message:',await page.locator('#remoteMessage').innerText());console.error('Page errors:',errors);throw error;});
        assert((await page.locator('#remotePunches').textContent()).includes('Khách hàng trình duyệt thử'));
        assert((await page.locator('#remotePunches').textContent()).includes('Đồng bộ trễ/offline'));
        // Local encrypted queue is durable across a normal authenticated reload and has been emptied after ack.
        await page.reload();await page.waitForFunction(()=>document.querySelector('#remoteQueue').textContent.includes('Không có bản ghi'));
        await page.setViewportSize({width:1440,height:1000});
        if(await page.evaluate(()=>document.body.classList.contains('hrm-menu-collapsed'))) await page.click('.hrm-menu-toggle');
        await page.waitForFunction(()=>Math.abs(document.querySelector('#leftsidebar').getBoundingClientRect().left)<1);
        assert(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth+1));
        assert.equal(await page.locator('.hrm-menu-toggle').getAttribute('aria-expanded'),'true');
        await page.evaluate(()=>scrollTo(0,0));
        await page.screenshot({path:path.join(process.env.HRM_REMOTE_TEST_OUTPUT,'remote-desktop.png'),animations:'disabled'});
        // Exercise the real form binder: decimal coordinates must survive the server locale.
        await page.selectOption('#planMode','HOME');
        await page.fill('#planPlace','Nhà kiểm thử tọa độ');
        await page.fill('#planLat','10.77');await page.fill('#planLng','106.69');
        const saved=page.waitForResponse(r=>r.url().endsWith('/RemoteAttendance/Plan') && r.request().method()==='POST');
        await page.locator('#remotePlanForm button[type=submit]').click();
        assert.equal((await saved).status(),200);
        await page.waitForFunction(()=>document.querySelector('#remotePlans').textContent.includes('Nhà kiểm thử tọa độ'));
        // Disabling GPS must allow capture but require an explanation before saving.
        await page.selectOption('#remotePlan',plan);
        await page.evaluate(()=>{ navigator.geolocation.getCurrentPosition=(_ok,fail)=>fail({code:1}); });
        await page.click('#remoteOpenCamera');await page.waitForFunction(()=>!document.querySelector('#remoteCapture').disabled,{},{timeout:10000}).catch(async error=>{console.error('Camera:',await page.locator('#remoteMessage').innerText());throw error;});
        await page.click('#remoteCapture');await page.waitForFunction(()=>document.querySelector('#remoteGps').textContent.includes('Chưa lấy được GPS'));
        await page.fill('#remoteNote','');await page.click('#remoteSend');
        assert((await page.locator('#remoteMessage').textContent()).includes('giải trình'));
        assert.deepEqual(errors,[]);
        console.log('PASS mobile/desktop layout, camera+GPS, offline encryption/sync, missing GPS explanation, zero page errors');
    } finally {await browser.close();}
})().catch(e=>{console.error(e);process.exitCode=1;});
