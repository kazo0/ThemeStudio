import fs from 'node:fs/promises';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const output = path.join(path.dirname(fileURLToPath(import.meta.url)), 'verification');
await fs.mkdir(output, { recursive: true });
const targets = await (await fetch('http://localhost:9227/json')).json();
const target = targets.find(item => item.type === 'page');
const socket = new WebSocket(target.webSocketDebuggerUrl);
await new Promise((resolve, reject) => {
	socket.addEventListener('open', resolve, { once: true });
	socket.addEventListener('error', reject, { once: true });
});
let nextId = 0;
const pending = new Map();
const events = [];
socket.addEventListener('message', event => {
	const message = JSON.parse(event.data);
	if (message.id) {
		const request = pending.get(message.id);
		if (request) {
			pending.delete(message.id);
			message.error ? request.reject(new Error(JSON.stringify(message.error))) : request.resolve(message.result);
		}
	} else if (['Runtime.consoleAPICalled', 'Runtime.exceptionThrown', 'Log.entryAdded', 'Network.loadingFailed'].includes(message.method)) {
		events.push(message);
	}
});
function send(method, params = {}) {
	return new Promise((resolve, reject) => {
		const id = ++nextId;
		pending.set(id, { resolve, reject });
		socket.send(JSON.stringify({ id, method, params }));
	});
}
const pause = ms => new Promise(resolve => setTimeout(resolve, ms));
await send('Runtime.enable');
await send('Page.enable');
await send('Log.enable');
await send('Network.enable');
const mode = process.argv[2] ?? 'load';
let name = mode;
if (mode === 'close') {
	await send('Browser.close');
	process.exit(0);
}
if (mode !== 'resize') {
	await send('Emulation.setDeviceMetricsOverride', { width: Number(process.argv[6] ?? 1440), height: Number(process.argv[7] ?? 1000), deviceScaleFactor: 1, mobile: false });
	await pause(600);
}
if (mode === 'load') {
	await send('Emulation.setDeviceMetricsOverride', { width: 1440, height: 1000, deviceScaleFactor: 1, mobile: false });
	await send('Page.navigate', { url: 'http://localhost:5127' });
	await pause(45000);
} else if (mode === 'click') {
	const x = Number(process.argv[3]);
	const y = Number(process.argv[4]);
	name = process.argv[5] ?? `click-${x}-${y}`;
	await send('Input.dispatchMouseEvent', { type: 'mousePressed', x, y, button: 'left', clickCount: 1 });
	await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x, y, button: 'left', clickCount: 1 });
	await pause(2500);
} else if (mode === 'text') {
	const x = Number(process.argv[3]);
	const y = Number(process.argv[4]);
	name = 'seed-change';
	await send('Input.dispatchMouseEvent', { type: 'mousePressed', x, y, button: 'left', clickCount: 1 });
	await send('Input.dispatchMouseEvent', { type: 'mouseReleased', x, y, button: 'left', clickCount: 1 });
	await send('Input.dispatchKeyEvent', { type: 'rawKeyDown', key: 'a', code: 'KeyA', windowsVirtualKeyCode: 65, modifiers: 2 });
	await send('Input.dispatchKeyEvent', { type: 'keyUp', key: 'a', code: 'KeyA', windowsVirtualKeyCode: 65, modifiers: 2 });
	await send('Input.insertText', { text: process.argv[5] });
	await pause(3000);
} else if (mode === 'resize') {
	const width = Number(process.argv[3]);
	const height = Number(process.argv[4]);
	name = `viewport-${width}`;
	await send('Emulation.setDeviceMetricsOverride', { width, height, deviceScaleFactor: 1, mobile: false });
	await pause(2000);
} else if (mode === 'evaluate') {
	const result = await send('Runtime.evaluate', { expression: process.argv[3], returnByValue: true, awaitPromise: true });
	console.log(JSON.stringify(result));
	socket.close();
	process.exit(0);
}
const capture = await send('Page.captureScreenshot', { format: 'png' });
await fs.writeFile(path.join(output, `browser-${name}.png`), Buffer.from(capture.data, 'base64'));
const state = await send('Runtime.evaluate', {
	expression: 'JSON.stringify({ title: document.title, body: document.body.innerText.slice(0, 6000), canvas: [...document.querySelectorAll("canvas")].map(c => ({width:c.width,height:c.height})), errors: window.__errors })',
	returnByValue: true
});
const summary = { mode, state, events };
await fs.writeFile(path.join(output, `browser-${name}.json`), JSON.stringify(summary, null, 2));
console.log(JSON.stringify({ mode, state: state.result?.value, errorCount: events.filter(e => e.method === 'Runtime.exceptionThrown' || e.params?.entry?.level === 'error').length, screenshot: path.join(output, `browser-${name}.png`) }));
socket.close();
