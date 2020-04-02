using System.Threading.Tasks;
using Grpc.Core;
using Ink.Runtime;
using System.Collections.Generic;
using System;
using InkGRPC;

namespace ink_runtime_grpc
{
        class ServerImpl : InkGRPC.Story.StoryBase
        {
        // stories contains a Dictionary of Guids to stories
        private Dictionary<string, Ink.Runtime.Story> stories;

        public ServerImpl()
        {
            stories = new Dictionary<string, Ink.Runtime.Story>();
        }
        
        public override Task<NewStoryReply> NewStory(NewStoryRequest request, ServerCallContext context)
        {
            string guid = Guid.NewGuid().ToString();
            Ink.Runtime.Story newStory;
            string json = request.Ink.ToString();
            try
            {
                newStory = new Ink.Runtime.Story(json);
                stories.Add(guid, newStory);
                return Task.FromResult(new NewStoryReply { Id = guid });
            }
            catch (Exception e)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Unable to create story: "+ e.Message));
            }
            
        }

        
        public override Task<ContinueReply> Continue(ContinueRequest request, ServerCallContext context)
        {

            Ink.Runtime.Story story = stories[request.Id];

            var storyState = MarshalStoryState(story);
            return Task.FromResult(new ContinueReply { Story=storyState });

        }

        public override Task<ChooseChoiceReply> ChooseChoice(ChooseChoiceRequest request, ServerCallContext context)
        {

            Ink.Runtime.Story story = stories[request.Id];
            var success = false;
            try
            {
                story.ChooseChoiceIndex(request.ChoiceIndex);
                success = true;
            }
            catch { }
            return Task.FromResult(new ChooseChoiceReply { Success = success });


        }


        private static InkGRPC.StoryState MarshalStoryState(Ink.Runtime.Story story)
        {
            var state = new InkGRPC.StoryState();
            state.Text = story.Continue();
            state.CanContinue = story.canContinue;
            foreach(Ink.Runtime.Choice choice in story.currentChoices)
            {
                state.Choices.Add(new InkGRPC.Choice { Index = choice.index, Text = choice.text });
            }
            foreach (string tag in story.currentTags)
            {
                state.CurrentTags.Add(tag);
            }
            foreach (string tag in story.globalTags)
            {
                state.GlobalTags.Add(tag);
            }
            return state;
        }
           
        }

   }
