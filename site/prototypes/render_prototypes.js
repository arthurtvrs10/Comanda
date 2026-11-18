const { execFileSync } = require('child_process');
const fs = require('fs');
const path = require('path');

const edgePath = 'C:\\Program Files (x86)\\Microsoft\\Edge\\Application\\msedge.exe';

const tasks = [
    {
        name: 'prototipo-hero-mockup.png',
        url: 'file:///' + path.resolve('prototypes/landing-page-vcomanda.html').replace(/\\/g, '/'),
        size: '1440,950'
    },
    {
        name: 'prototipo-landing-desktop.png',
        url: 'file:///' + path.resolve('prototypes/landing-page-vcomanda.html').replace(/\\/g, '/'),
        size: '1440,4600'
    },
    {
        name: 'prototipo-landing-mobile.png',
        url: 'file:///' + path.resolve('prototypes/landing-page-vcomanda.html').replace(/\\/g, '/'),
        size: '390,4800'
    },
    {
        name: 'prancha-design-system.png',
        url: 'file:///' + path.resolve('prototypes/design-system-board.html').replace(/\\/g, '/'),
        size: '1440,1150'
    }
];

tasks.forEach(task => {
    const outPath = path.resolve('prototypes', task.name);
    console.log(`Rendering ${task.name} at ${task.size}...`);
    try {
        execFileSync(edgePath, [
            '--headless=new',
            '--screenshot=' + outPath,
            '--window-size=' + task.size,
            '--hide-scrollbars',
            '--disable-gpu',
            '--virtual-time-budget=2000',
            task.url
        ]);
        if (fs.existsSync(outPath)) {
            const sizeKB = (fs.statSync(outPath).size / 1024).toFixed(1);
            console.log(`✓ Generated ${task.name} (${sizeKB} KB)`);
        } else {
            console.error(`✗ Failed to create ${task.name}`);
        }
    } catch (e) {
        console.error(`Error rendering ${task.name}:`, e.message);
    }
});
