open OpenAI
open OpenAI.Chat
open System
open System.ClientModel

let getEnv defaultValue name =
    match Environment.GetEnvironmentVariable(name) with
    | null | "" -> defaultValue
    | v -> v

let requireApiKey () =
    match Environment.GetEnvironmentVariable "OPENROUTER_API_KEY" with
    | null | "" -> failwith "OPENROUTER_API_KEY is not set"
    | v -> v

let parseArgs (argv: string[]) =
    match argv |> Array.toList with
    | "-p" :: prompt :: _ when not (String.IsNullOrWhiteSpace prompt) -> prompt
    | _ -> failwith "Usage: program -p <prompt>"

let createClient apiKey baseUrl =
    let options = OpenAIClientOptions(Endpoint = Uri(baseUrl))
    ChatClient(model = "anthropic/claude-haiku-4.5", credential = ApiKeyCredential(apiKey), options = options)

let callModel (client: ChatClient) (prompt : string) =
    let message: ChatMessage = UserChatMessage prompt
    client.CompleteChat(messages = [| message |])

let handleResponse (response: ClientResult<ChatCompletion>) =
    if isNull response.Value.Content || response.Value.Content.Count = 0 then
        failwith "No choices in response"
    else
        Console.Error.WriteLine("Logs from your program will appear here!")
        response.Value.Content.[0].Text

[<EntryPoint>]
let main argv =
    try
        let prompt = parseArgs argv
        let apiKey = requireApiKey()
        let baseUrl = getEnv "https://openrouter.ai/api/v1" "OPENROUTER_BASE_URL"
        let client = createClient apiKey baseUrl
        let response = callModel client prompt
        let text = handleResponse response
        printf "%s" text
        0
    with ex ->
        Console.Error.WriteLine(ex.Message)
        1
