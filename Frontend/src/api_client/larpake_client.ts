import HttpClient from "./http_client.ts";
import { Larpake } from "../data_model/larpake.ts";


export default class LarpakeClient {
    client: HttpClient;

    constructor() {
        this.client = new HttpClient();
    }

    async getOwn() : Promise<Larpake | null> {
        const response = await this.client.get("api/larpakkeet/own")
        if (!response.ok){
            return null;
        }
        return await response.json();
    }
}
