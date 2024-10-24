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
    private readonly CONCURRENT_PAGES = 15; // Adjust based on your needs and server capacity
    private readonly BROWSER_WS = "wss://brd-customer-hl_8e4e9ffe-zone-scraping_browser1:s86gsxq17cjo@brd.superproxy.io:9222";

    private async createBrowserPage() {
        const browser = await puppeteer.connect({
            browserWSEndpoint: this.BROWSER_WS,
        });
        return await browser.newPage();
    }

    private cleanAndFixJson(jsonString: string): any {
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
    }

    private async scrapePage(search: string, pageNumber: number): Promise<Ad[]> {
        const page = await this.createBrowserPage();
        try {
            const url = `https://www.milanuncios.com/anuncios/?s=${encodeURIComponent(search)}&orden=relevance&fromSearch=1&hitOrigin=home_search&pagina=${pageNumber}`;
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

    private generateScrapeJobs(searchTerms: string[], pagesToScrap: number): Array<{ search: string, page: number }> {
        const jobs: Array<{ search: string, page: number }> = [];
        for (const search of searchTerms) {
            for (let page = 1; page <= pagesToScrap; page++) {
                jobs.push({ search, page });
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

    private async scrapeInBatches(jobs: Array<{ search: string, page: number }>) {
        const results: Ad[] = [];
        
        for (let i = 0; i < jobs.length; i += this.CONCURRENT_PAGES) {
            const batch = jobs.slice(i, i + this.CONCURRENT_PAGES);
            const batchResults = await Promise.all(
                batch.map(job => this.scrapePage(job.search, job.page))
            );
            
            results.push(...batchResults.flat());
            
            // Add a small delay between batches to avoid overwhelming the server
            if (i + this.CONCURRENT_PAGES < jobs.length) {
                await new Promise(resolve => setTimeout(resolve, 2000));
            }
        }
        
        return results;
    }

    /**
     * Scrapes ads from MilAnuncios based on search terms
     * @param requestBody Contains search terms and number of pages to scrape
     */
    @Post('/')
    public async scrapeAds(@Body() requestBody: ScrapingRequest): Promise<Ad[]> {
        const { searchTerms, pagesToScrap } = requestBody;
        
        // Generate all scraping jobs
        const jobs = this.generateScrapeJobs(searchTerms, pagesToScrap);
        
        // Execute scraping jobs in parallel batches
        const allAds = await this.scrapeInBatches(jobs);
        
        // Remove duplicates based on ad ID
        const uniqueAds = Array.from(
            new Map(allAds.map(ad => [ad.id, ad])).values()
        );
        
        return uniqueAds;
    }
}