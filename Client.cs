using System;
using System.IO;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using InkGRPC;

namespace ink_runtime_grpc
{
    public class InkClient
    {
        const string file = "/Users/alex/git/ink/ink-runtime-grpc/Client/test.ink.json";
        public static void Run()
        {
            Channel channel = new Channel("127.0.0.1:50051", ChannelCredentials.Insecure);

            var client = new Story.StoryClient(channel);
            string ink;
            using (StreamReader r = new StreamReader(file))
            {
                ink = r.ReadToEnd();

            }
            Google.Protobuf.WellKnownTypes.Struct inkStruct = new Struct();
            inkStruct = Google.Protobuf.WellKnownTypes.Struct.Parser.ParseJson(ink);


			try
			{
                var reply = client.NewStory(new NewStoryRequest { Ink = inkStruct });
                var id = reply.Id;
                Console.WriteLine("StoryId: " + id);
                var canContinue = true;
                while (true)
                {
                    StoryState state = new StoryState();
                    while (canContinue == true)
                    {
                        var continueReply = client.Continue(new ContinueRequest { Id = id });
                        state = continueReply.Story;
                        var continueText = state.Text;
                        Console.WriteLine(continueText);
                        canContinue = state.CanContinue;
                        foreach (Choice choice in state.Choices)
                        {
                            Console.WriteLine(choice.Index + 1 + ": " + choice.Text);
                        }

                    }
                    if (state.Choices.Count > 0){
                        client.ChooseChoice(new ChooseChoiceRequest { Id = id, ChoiceIndex = 0 });
                        canContinue = true;
                    }
                    else { break; }
                    
                }                

            }
            catch (RpcException e)
			{
                Console.WriteLine(e.Status.Detail);
                Console.WriteLine(e.Status.StatusCode);
                Console.WriteLine((int)e.Status.StatusCode);
            }            

            channel.ShutdownAsync().Wait();
        }
    }
}
