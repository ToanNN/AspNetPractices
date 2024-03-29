import * as signalR from "@microsoft/signalr";
import * as signalRMsgPack from "@microsoft/signalr-protocol-msgpack";

import "./css/main.css";


const divMessages: HTMLDivElement = document.querySelector("#divMessages");
const tbMessage: HTMLInputElement = document.querySelector("#tbMessage");
const btnSend: HTMLButtonElement = document.querySelector("#btnSend");
const username = new Date().getTime();

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/hub")
    //.withHubProtocol(new signalRMsgPack.MessagePackHubProtocol())
    .withAutomaticReconnect()
    .configureLogging(signalR.LogLevel.Debug)
    .build();

connection.on("ReceiveMessage", (username: string, message: string) => {
    const m = document.createElement("div");

    m.innerHTML = `<div class="message-author">${username}</div><div>${message}</div>`;

    divMessages.appendChild(m);
    divMessages.scrollTop = divMessages.scrollHeight;
});

connection.start().catch(err => document.write(err));

connection.on("GetMessageFromClient",
    async() => {
        let promise = new Promise((resolve, reject) => {
            setTimeout(() => {
                    resolve("Here is your message");
                },
                100);
        });
        return promise;
    });

tbMessage.addEventListener("keyup", (e: KeyboardEvent) => {
    if (e.key === "Enter") {
        send();
    }
});

btnSend.addEventListener("click", send);

function send() {
    connection.send("acceptMessages", username, tbMessage.value)
        .then(() => tbMessage.value = "")
        .catch(function(err) {
            console.error(err);
        });
}
