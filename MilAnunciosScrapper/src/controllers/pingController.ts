import { Body, Controller, Post, Route } from 'tsoa';
import puppeteerExtra from 'puppeteer-extra';
import * as puppeteer from 'puppeteer-core';
import StealthPlugin from 'puppeteer-extra-plugin-stealth';

// Use Puppeteer Stealth plugin to avoid bot detection
puppeteerExtra.use(StealthPlugin());

// Interfaces

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

export interface ScrapingRequest {
    searchTerms: string[];
    pagesToScrap: number;
    category?: string; // Nuevo parámetro opcional
}

// Mantener las demás interfaces y clases igual...

@Route('scraping')
export class ScrapingController extends Controller {
    private readonly CONCURRENT_PAGES = 15;
    private readonly BROWSER_WS = "wss://brd-customer-hl_959f9d49-zone-scraping_browser1:52skc782mq4u@brd.superproxy.io:9222";

    private async createBrowserPage() {
        const browser = await puppeteerExtra.connect({
            browserWSEndpoint: this.BROWSER_WS,
        });
        const page = await browser.newPage();
        await page.setRequestInterception(true);
        page.on('request', (request: puppeteer.HTTPRequest) => {
            if (['image', 'stylesheet'].includes(request.resourceType())) {
                request.abort();
            } else {
                request.continue();
            }
        });

        return page;
    }

    private cleanAndFixJson(jsonString: string): any {
        try {
            let cleanedJson = jsonString
                .replace(/\\\\/g, '\\')
                .replace(/\\"/g, '"')
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
    }

    private async scrapePage(search: string, pageNumber: number, category?: string): Promise<Ad[]> {
        const page = await this.createBrowserPage();
        try {
            let url = `https://www.milanuncios.com/anuncios/?s=${encodeURIComponent(search)}&orden=relevance&fromSearch=1&hitOrigin=home_search&pagina=${pageNumber}`;
            if (category) {
                url += `&category=${encodeURIComponent(category)}`;
            }

            await page.goto(url, { waitUntil: 'domcontentloaded', timeout: 60000 });
            
            // Random delay between 1-3 seconds
            await new Promise(resolve => setTimeout(resolve, Math.random() * 2000 + 1000));
            
            const pageContent = await page.content();
            let pFrom = pageContent.indexOf('{\\\"ads\\\":');
            let pTo = pageContent.indexOf('}]}', pFrom + 1);
            let jsonString = pageContent.substring(pFrom, pTo + 3);

            const jsonObject = this.cleanAndFixJson(jsonString);
            console.log(`Scraped search: ${search}, page: ${pageNumber}`);
            
            return jsonObject?.ads || [];
        } catch (error) {
            console.error(`Error scraping search: ${search}, page: ${pageNumber}:`, error);
            return [];
        } finally {
            await page.browser().close();
        }
    }

    private generateScrapeJobs(searchTerms: string[], pagesToScrap: number, category?: string): Array<{ search: string, page: number, category?: string }> {
        const jobs: Array<{ search: string, page: number, category?: string }> = [];
        for (const search of searchTerms) {
            for (let page = 1; page <= pagesToScrap; page++) {
                jobs.push({ search, page, category });
            }
        }
        return this.shuffleArray(jobs);
    }

    private shuffleArray<T>(array: T[]): T[] {
        for (let i = array.length - 1; i > 0; i--) {
            const j = Math.floor(Math.random() * (i + 1));
            [array[i], array[j]] = [array[j], array[i]];
        }
        return array;
    }

    private async scrapeInBatches(jobs: Array<{ search: string, page: number, category?: string }>) {
        const results: Ad[] = [];
        
        for (let i = 0; i < jobs.length; i += this.CONCURRENT_PAGES) {
            const batch = jobs.slice(i, i + this.CONCURRENT_PAGES);
            const batchResults = await Promise.all(
                batch.map(job => this.scrapePage(job.search, job.page, job.category))
            );
            
            results.push(...batchResults.flat());
            
            if (i + this.CONCURRENT_PAGES < jobs.length) {
                await new Promise(resolve => setTimeout(resolve, 2000));
            }
        }
        
        return results;
    }

    /**
     * Scrapes ads from MilAnuncios based on search terms and optional category
     * @param requestBody Contains search terms, number of pages to scrape, and optional category
     */
    @Post('/')
    public async scrapeAds(@Body() requestBody: ScrapingRequest): Promise<Ad[]> {
        const { searchTerms, pagesToScrap, category } = requestBody;
        
        // Generate all scraping jobs
        const jobs = this.generateScrapeJobs(searchTerms, pagesToScrap, category);
        
        // Execute scraping jobs in parallel batches
        const allAds = await this.scrapeInBatches(jobs);
        
        // Remove duplicates based on ad ID
        const uniqueAds = Array.from(
            new Map(allAds.map(ad => [ad.id, ad])).values()
        );
        
        return uniqueAds;
    }
}
