﻿using FantasyChas_Backend.Data;
using FantasyChas_Backend.Models;
using FantasyChas_Backend.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenAI_API;
using OpenAI_API.Models;

namespace FantasyChas_Backend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<ChatController> _logger;
        private static ApplicationDbContext _context;


        public ChatController(ILogger<ChatController> logger, UserManager<IdentityUser> userManager, ApplicationDbContext context)
        {
            _logger = logger;

            // Added UserManager to get access to the correct user in the entire DisplayCharacterController.
            _userManager = userManager;
            _context = context;

        }


        [HttpPost("/getAnswerFromChatGPT")]
        public async Task<IActionResult> GetAnswerFromChatGPT(string query, OpenAIAPI api)
        {
            IdentityUser user = await GetCurrentUserAsync();

            var chat = api.Chat.CreateConversation();
            chat.Model = Model.ChatGPTTurbo;
            chat.RequestParameters.Temperature = 0;

            chat.AppendSystemMessage("Du är en dungeon master för spelet Dungeons and Dragons. Du kommer att få en prompt med en historia som du ska fortsätta på och kommer att interagera med en karaktär som jag kommer att använda i den värld du bygger upp. Du får endast använda människa som ras för karaktärer som dyker upp. De måste ha klasser som passar in i den verkliga världen. Magi finns inte i detta scenario. Istället för att slå tärning för att avgöra hur saker går, räcker det att du bestämmer hur det går.\r\nHistoria: Du ska utgå från broschyren \"Om krisen eller kriget kommer\" när du fortsätter den här historian. Få med sådant som är viktigt att tänka på i ett riktigt överlevnadsscenario. Här är bakgrunden som du ska bygga vidare på: Det har varit oroliga tider i världen de senaste åren. Flertalet stora konflikter har blossat upp runtom i världen. Klimatkrisen har lett till att vädret är extremt ostabilt. Detta i kombination med interna stridigheter inom olika länders riksdagar har lett till en stor ekonomisk instabilitet i de allra flesta länder.");


            // give a few examples as user and assistant
            chat.AppendUserInput("Min karaktär: Namn: Pelle Pellesson, HP: 10, Yrke: Kassapersonal på ICA, Ras: Människa, Level: 1, Ålder: 37, Attributes: Styrka: 12, Smidighet: 14, Intelligens: 10, Vishet: 8, Karisma: 15, Constitution: 11, Bakgrund: Jag kommer ursprungligen från en liten ort i mellersta Sverige, men bor nu i Stockholm. Jag har sedan jag var liten tyckt om tv-spel och klassiska bilar. Jag bor i ett attefallshus på en släktings tomt i en förort till Stockholm. Jag har ofta lätt att bli arg, men har gått i terapi för det i 5 år nu. Jag har ett par goda vänner som jag brukar ses med på lördagar för att spela DnD. Mina föräldrar bor kvar i mitt hemort. Jag har inga syskon.");
            chat.AppendExampleChatbotOutput("Du väcks tidigt på morgonen av att sirener ljuder över området. Vad gör du?");



            var chatHistory = _context.ChatHistories
                    .Select(ch => new { ch.Prompt, ch.Answer });
            foreach (var chatRow in chatHistory)
            {
                chat.AppendUserInput(chatRow.Prompt);
                chat.AppendExampleChatbotOutput(chatRow.Answer);
            }

            // nu innan vi hanterar active story på riktigt
            Character? activeCharacter= await _context.Characters
                .OrderBy(a => a.Id)
                .LastOrDefaultAsync();
            if (activeCharacter is null)
            {
                activeCharacter = new Character() { User = user, Name = "The survivor" };
                await _context.Characters.AddAsync(activeCharacter);
                await _context.SaveChangesAsync();
            }
            ActiveStory? activeStory = await _context.ActiveStories
                .OrderBy(a => a.Id)
                .LastOrDefaultAsync();
            if (activeStory is null)
            {
                activeStory = new ActiveStory() { User = user, Name = "stayin alive", BasePrompt = "tratatta" , Character=activeCharacter};
                await _context.ActiveStories.AddAsync(activeStory);
                await _context.SaveChangesAsync();
            }

            chat.AppendUserInput(query);
            var answer = await chat.GetResponseFromChatbotAsync();
            ChatHistory chatSave = new ChatHistory()
            {
                Answer = answer,
                Prompt = query,
                ActiveStory = activeStory,
                Character = activeCharacter,
                Timestamp = DateTime.Now
            };
            await _context.ChatHistories.AddAsync(chatSave);
            await _context.SaveChangesAsync();

            return Ok(answer);

        }
        private Task<IdentityUser> GetCurrentUserAsync() => _userManager.GetUserAsync(User);

    }
}
