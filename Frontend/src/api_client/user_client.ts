import HttpClient from "./http_client.ts";

export class UserClient{
    client: HttpClient;
    
    constructor(){
        this.client = new HttpClient();
    }
}