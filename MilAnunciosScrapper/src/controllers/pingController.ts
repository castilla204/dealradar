import { Body, Controller, Post, Route } from 'tsoa';
import puppeteer from 'puppeteer-extra';
import StealthPlugin from 'puppeteer-extra-plugin-stealth';

// Use Puppeteer Stealth plugin to avoid bot detection
puppeteer.use(StealthPlugin());

// Interfaces
export interface ScrapingRequest {
    searchTerms: string[];
    pagesToScrap: number;
}

export interface Ad {
    category: Category;
    categoryId: number;
    categoryTree: CategoryTree[];
    city: City;
    contactable: boolean;
    description: string;
    highlighted: boolean;
    id: string;
    images: string[];
    isNew: boolean;
    isReserved: string;
    location: Location;
    origin: Origin;
    price: Price;
    province: Province2;
    publishDate: string;
    searchLink: SearchLink;
    sellerType: string;
    sellType: string;
    seoTitle: string;
    tags: Tag[];
    title: string;
    url: string;
    userId: number;
    sortDate: string;
    updateDate: string;
    shippingType?: string;
}

export interface Category {
    id: number;
    name: string;
    slug: string;
}

export interface CategoryTree {
    id: number;
    name: string;
    slug: string;
}

export interface City {
    id: number;
    name: string;
    slug: string;
}

export interface Location {
    city: City2;
    province: Province;
    region: Region;
}

export interface City2 {
    id: number;
    name: string;
    slug: string;
}

export interface Province {
    id: number;
    name: string;
    slug: string;
}

export interface Region {
    id: number;
    name: string;
    slug: string;
}

export interface Origin {
    name: string;
    provider?: string;
}

export interface Price {
    cashPrice: CashPrice;
}

export interface CashPrice {
    value: number;
    includeTaxes?: boolean;
}

export interface Province2 {
    id: number;
    name: string;
    slug: string;
}

export interface SearchLink {
    label: string;
    url: string;
}

export interface Tag {
    type: string;
    text: string;
}

@Route('scraping')
export class ScrapingController extends Controller {
    /**
     * Scrapes ads from MilAnuncios based on search terms
     * @param requestBody Contains search terms and number of pages to scrape
     */
    @Post('/')
    public async scrapeAds(@Body() requestBody: ScrapingRequest): Promise<Ad[]> {
        const { searchTerms, pagesToScrap } = requestBody;
        
        // Configuration for browser connection via proxy
        const BROWSER_WS = "wss://brd-customer-hl_83e6708b-zone-scraping_browser1:slm7mxu8j1r5@brd.superproxy.io:9222";

        const browser = await puppeteer.connect({
            browserWSEndpoint: BROWSER_WS,
        });

        const page = await browser.newPage();
        let allAds: Ad[] = [];

        // Tracking objects
        let searchPageCounts: { [key: string]: number } = {};
        let scrapedPages: { [key: string]: Set<number> } = {};
        searchTerms.forEach(search => {
            searchPageCounts[search] = 0;
            scrapedPages[search] = new Set();
        });

        const getNextSearch = (currentIndex: number): string => {
            return searchTerms[currentIndex % searchTerms.length];
        };

        const getRandomPageNumber = (search: string): number | null => {
            const availablePages = Array.from({ length: pagesToScrap }, (_, i) => i + 1)
                .filter(page => !scrapedPages[search].has(page));

            if (availablePages.length === 0) {
                return null;
            }

            const randomPage = availablePages[Math.floor(Math.random() * availablePages.length)];
            scrapedPages[search].add(randomPage);
            return randomPage;
        };

        const cleanAndFixJson = (jsonString: string): any => {
            try {
                let cleanedJson = jsonString
                    .replace(/\\\\/g, '\\')
                    .replace(/\\\"/g, '"')
                    .replace(/\\n/g, ' ')
                    .replace(/\\r/g, ' ')
                    .replace(/\\t/g, ' ');

                cleanedJson = cleanedJson.replace(/"([^"]*?)"/g, (match, p1) => {
                    const escapedString = p1.replace(/"/g, '\\"');
                    return `"${escapedString}"`;
                });

                cleanedJson = cleanedJson.replace(/\\u([0-9A-Fa-f]{4})/g, (match, p1) => {
                    return String.fromCharCode(parseInt(p1, 16));
                });

                cleanedJson = cleanedJson
                    .replace(/€|\\u20AC/g, '€')
                    .replace(/�/g, '');

                return JSON.parse(cleanedJson);
            } catch (error) {
                console.error("Error trying to fix JSON:", error);
                return null;
            }
        };

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

                const jsonObject = cleanAndFixJson(jsonString);

                if (jsonObject && jsonObject.ads) {
                    allAds = allAds.concat(jsonObject.ads);
                }

                console.log(`Scraped search: ${search}, page: ${pageNumber}`);
            } catch (error) {
                console.error(`Error scraping search: ${search}, page: ${pageNumber}`, error);
            }

            currentSearchIndex++;
        }

        await browser.close();
        return allAds;
    }
}
