import express, { Request, Response } from 'express';
import puppeteer from 'puppeteer-extra';
import StealthPlugin from 'puppeteer-extra-plugin-stealth';

puppeteer.use(StealthPlugin());

const app = express();
const port = 5000;

// Lista para almacenar las cookies obtenidas
let cookiesList: string[] = [];

// FunctiontoObtainWebCookies
async function fetchCookies(): Promise<void> {
    const BROWSER_WS = "wss://brd-customer-hl_83e6708b-zone-scraping_browser1:slm7mxu8j1r5@brd.superproxy.io:9222";
    
    console.log("Connecting to browser...");
    const browser = await puppeteer.connect({
        browserWSEndpoint: BROWSER_WS,
    });

    const newCookies: string[] = [];

    // do 3 visits
    for (let i = 0; i < 3; i++) {
        console.log(`Visit ${i + 1}: Navigating to Vinted...`);
        const page = await browser.newPage();
        await page.goto('https://www.vinted.es/', { waitUntil: 'networkidle0' });

        const cookies = await page.cookies();
        newCookies.push(JSON.stringify(cookies));  // Almacenar las cookies en formato JSON
        await page.close();
    }

    await browser.close();

    // update cookies
    cookiesList = newCookies;
    console.log('Cookies updated:', cookiesList);
}

// get old cookie
function getRandomCookie(): string | null {
    if (cookiesList.length === 0) {
        return null;  // Si no hay cookies disponibles
    }
    const randomIndex = Math.floor(Math.random() * cookiesList.length);
    return cookiesList[randomIndex];
}

// Update every 10 minutes
setInterval(async () => {
    try {
        console.log('Fetching new cookies...');
        await fetchCookies();
        console.log('Cookies fetched and updated.');
    } catch (error) {
        console.error('Error fetching cookies:', error);
    }
}, 10 * 60 * 1000);  

// Do fetch cookies when program start
(async () => {
    try {
        console.log('Fetching cookies on startup...');
        await fetchCookies();
        console.log('Initial cookies fetched and stored.');
    } catch (error) {
        console.error('Error fetching cookies on startup:', error);
    }
})();

// Get random cookie
app.get('/cookies', async (_req: Request, res: Response) => {
    try {
        const randomCookie = getRandomCookie();
        if (randomCookie) {
            res.send(randomCookie);
        } else {
            res.status(500).send('No cookies available at the moment');
        }
    } catch (error) {
        console.error('Error sending cookie:', error);
        res.status(500).send('An error occurred while sending the cookie');
    }
});

// Iniciar el servidor
app.listen(port, () => {
    console.log(`Server running at http://localhost:${port}`);
});
