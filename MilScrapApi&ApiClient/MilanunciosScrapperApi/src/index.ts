import express from 'express';

import { Console } from 'console';
import puppeteer from 'puppeteer-extra';
import StealthPlugin from 'puppeteer-extra-plugin-stealth';
import { json } from 'body-parser';
// Use Puppeteer Stealth plugin to avoid bot detection
puppeteer.use(StealthPlugin());

const app = express();
const port = 3000;
app.use(express.json());


// Interfaces for ad data structure
export interface Root {
    ads: Ad[]
}

export interface Ad {
    category: Category
    categoryId: number
    categoryTree: CategoryTree[]
    city: City
    contactable: boolean
    description: string
    highlighted: boolean
    id: string
    images: string[]
    isNew: boolean
    isReserved: string
    location: Location
    origin: Origin
    price: Price
    province: Province2
    publishDate: string
    searchLink: SearchLink
    sellerType: string
    sellType: string
    seoTitle: string
    tags: Tag[]
    title: string
    url: string
    userId: number
    sortDate: string
    updateDate: string
    shippingType?: string
}

export interface Category {
    id: number
    name: string
    slug: string
}

export interface CategoryTree {
    id: number
    name: string
    slug: string
}

export interface City {
    id: number
    name: string
    slug: string
}

export interface Location {
    city: City2
    province: Province
    region: Region
}

export interface City2 {
    id: number
    name: string
    slug: string
}

export interface Province {
    id: number
    name: string
    slug: string
}

export interface Region {
    id: number
    name: string
    slug: string
}

export interface Origin {
    name: string
    provider?: string
}

export interface Price {
    cashPrice: CashPrice
}

export interface CashPrice {
    value: number
    includeTaxes?: boolean
}

export interface Province2 {
    id: number
    name: string
    slug: string
}

export interface SearchLink {
    label: string
    url: string
}

export interface Tag {
    type: string
    text: string
}

async function scrapeAds(searchTerms: string[], pagesToScrap: number): Promise<Ad[]> {
    // Configuration for browser connection via proxy
    const BROWSER_WS = "wss://brd-customer-hl_d27bbe0e-zone-scraping_browser1:yg0dktbm74f0@brd.superproxy.io:9222";

    console.log("Connecting to browser...");
    const browser = await puppeteer.connect({
        browserWSEndpoint: BROWSER_WS,
    });

    console.log("Connected! Navigate to site...");

    const page = await browser.newPage();

    // Initialize the list of ads
    let allAds: Ad[] = [];

    // Create an object to keep track of pages scraped per search
    let searchPageCounts: { [key: string]: number } = {};
    let scrapedPages: { [key: string]: Set<number> } = {};
    searchTerms.forEach(search => {
        searchPageCounts[search] = 0;
        scrapedPages[search] = new Set();
    });

    // Function to get the next search term in a circular manner
    function getNextSearch(currentIndex: number): string {
        return searchTerms[currentIndex % searchTerms.length];
    }

    // Function to get a random unused page number for a specific search
    function getRandomPageNumber(search: string): number | null {
        const availablePages = Array.from({ length: pagesToScrap }, (_, i) => i + 1)
            .filter(page => !scrapedPages[search].has(page));

        if (availablePages.length === 0) {
            return null;
        }

        const randomPage = availablePages[Math.floor(Math.random() * availablePages.length)];
        scrapedPages[search].add(randomPage);
        return randomPage;
    }

    // Function to clean and fix JSON
    function cleanAndFixJson(jsonString: string): any {
        try {
            // Step 1: Replace problematic escape sequences
            let cleanedJson = jsonString.replace(/\\\\/g, '\\');
            cleanedJson = cleanedJson.replace(/\\\"/g, '"');
            cleanedJson = cleanedJson.replace(/\\n/g, ' ');
            cleanedJson = cleanedJson.replace(/\\r/g, ' ');
            cleanedJson = cleanedJson.replace(/\\t/g, ' ');

            // Step 2: Properly escape double quotes within text property values
            cleanedJson = cleanedJson.replace(/"([^"]*?)"/g, (match, p1) => {
                const escapedString = p1.replace(/"/g, '\\"');
                return `"${escapedString}"`;
            });

            // Step 3: Fix malformed Unicode sequences
            cleanedJson = cleanedJson.replace(/\\u([0-9A-Fa-f]{4})/g, (match, p1) => {
                return String.fromCharCode(parseInt(p1, 16));
            });

            // Step 4: Replace problematic symbols
            cleanedJson = cleanedJson.replace(/€|\\u20AC/g, '€');
            cleanedJson = cleanedJson.replace(/�/g, '');

            // Step 5: Try to parse the cleaned JSON
            const parsedJson = JSON.parse(cleanedJson);
            return parsedJson;
        } catch (error) {
            console.error("Error trying to fix JSON:", error);
            return null;
        }
    }

    let currentSearchIndex = 0;
    let totalScrapedPages = 0;
    const totalPagesToScrap = searchTerms.length * pagesToScrap;

    while (totalScrapedPages < totalPagesToScrap) {
        const search = getNextSearch(currentSearchIndex);
        const pageNumber = getRandomPageNumber(search);

        if (pageNumber === null) {
            currentSearchIndex++;
            continue;
        }

        try {
            searchPageCounts[search]++;
            totalScrapedPages++;

            const url = `https://www.milanuncios.com/anuncios/?s=${search}&orden=relevance&fromSearch=1&hitOrigin=home_search&pagina=${pageNumber}`;
            await page.goto(url, { waitUntil: 'domcontentloaded', timeout: 60000 });
            const pageContent = await page.content();
            await new Promise(resolve => setTimeout(resolve, Math.random() * 4000 + 3000));

            let pFrom = pageContent.indexOf('{\\\"ads\\\":');
            let pTo = pageContent.indexOf('}]}', pFrom + 1);
            let jsonString = pageContent.substring(pFrom, pTo + 3);

            // Clean and parse the JSON
            const jsonObject = cleanAndFixJson(jsonString);

            // Add the ads from the current page to the common list
            if (jsonObject && jsonObject.ads) {
                allAds = allAds.concat(jsonObject.ads);
            }

            console.log(`Scraped search: ${search}, page: ${pageNumber}`);
        } catch (error) {
            console.error(`Error scraping search: ${search}, page: ${pageNumber}`, error);
        }

        currentSearchIndex++;
    }

    console.log("Scraping completed. Total ads:", allAds.length);

    await browser.close();
    return allAds;
}

app.get('/', (_req, res) => {
  res.send('Hello Typescript world!');
});

// const PORT = process.env.PORT || 5000;
// app.listen(PORT, () => {
//   console.log(`Listening on port ${PORT}`);
// });


app.post('/scrape', async (req, res) => {
    try {
        const { searchTerms, pagesToScrap } = req.body;
        
        if (!Array.isArray(searchTerms) || typeof pagesToScrap !== 'number') {
            return res.status(400).json({ error: 'Invalid input. Please provide an array of search terms and a number of pages to scrape.' });
        }

        const scrapedAds = await scrapeAds(searchTerms, pagesToScrap);
        res.json(scrapedAds);
    } catch (error) {
        console.error('Error during scraping:', error);
        res.status(500).json({ error: 'An error occurred during scraping.' });
    }
});

app.listen(port, () => {
    console.log(`Server running at http://localhost:${port}`);
});

