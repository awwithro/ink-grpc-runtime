using System.Threading.Tasks;
using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using System.Collections.Generic;
using System;
using InkGRPC;

namespace ink_runtime_grpc
{
    class InkServer : Story.StoryBase
    {
        // contains a Dictionary of Guids to stories
        private Dictionary<string, Ink.Runtime.Story> runningStories;
        // dictionary of story titles to ink json
        private Dictionary<string, string> stories;
        private string storyPath;
        public InkServer(string StoryPath)
        {
            runningStories = new Dictionary<string, Ink.Runtime.Story>();
            stories = new Dictionary<string, string>();
            storyPath = StoryPath;
        }

        public override Task<NewStoryReply> NewStory(NewStoryRequest request, ServerCallContext context)
        {
            string guid = Guid.NewGuid().ToString();
            Ink.Runtime.Story newStory;
            string json = request.Ink.ToString();
            try
            {
                newStory = new Ink.Runtime.Story(json);
                
                runningStories.Add(guid, newStory);
                return Task.FromResult(new NewStoryReply { Id = guid });
            }
            catch (Exception e)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Unable to create story: " + e.Message));
            }

        }
        public override Task<NewStoryReply> StartStory(StartStoryRequest request, ServerCallContext context)
        {
            string guid = Guid.NewGuid().ToString();
            Ink.Runtime.Story newStory;
            try
            {
                newStory = new Ink.Runtime.Story(stories[request.StoryTitle]);
                runningStories.Add(guid, newStory);
                return Task.FromResult(new NewStoryReply { Id = guid });
            }
            catch (Exception e)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Unable to start story: " + e.Message));
            }

        }
        public override Task<ListStoriesReply> ListStories(Empty request, ServerCallContext context)
        {

            try
            {
                var reply = new ListStoriesReply();
                foreach (string title in stories.Keys)
                {
                    reply.StoryTitles.Add(title);
                }
                return Task.FromResult(reply);
            }
            catch (Exception e)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Unable to list story: " + e.Message));
            }

        }


        public override Task<ContinueReply> Continue(ContinueRequest request, ServerCallContext context)
        {
            try
            {
                Ink.Runtime.Story story = runningStories[request.Id];
                var storyState = MarshalStoryState(story);
                return Task.FromResult(new ContinueReply { Story = storyState });
            }
            catch (Exception e)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Unable to continue story: " + e.Message));
            }

        }
        public override Task<Empty> ChoosePathString(ChoosePathStringRequest request, ServerCallContext context)
        {
            try
            {
                Ink.Runtime.Story story = runningStories[request.Id];
                story.ChoosePathString(request.Path, request.ResetCallstack);
                return Task.FromResult(new Empty());
            }
            catch (Exception e)
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Unable to continue story: " + e.Message));
            }

        }


        public override Task<ChooseChoiceReply> ChooseChoice(ChooseChoiceRequest request, ServerCallContext context)
        {

            Ink.Runtime.Story story = runningStories[request.Id];
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
            foreach (Ink.Runtime.Choice choice in story.currentChoices)
            {
                state.Choices.Add(new InkGRPC.Choice { Index = choice.index, Text = choice.text });
            }
            foreach (string tag in story.currentTags)
            {
                state.CurrentTags.Add(tag);
            }
            if (story.globalTags != null)
            {
                foreach (string tag in story.globalTags)
                {
                    state.GlobalTags.Add(tag);
                }
            }
            return state;
        }



        public int LoadStories()
        {
            var files = System.IO.Directory.GetFiles(storyPath, "*.json");
            int loadedFiles = 0;
            foreach (string file in files)
            {
                string title = System.IO.Path.GetFileNameWithoutExtension(file);
                string story = System.IO.File.ReadAllText(file);
                loadedFiles +=1;
                stories[title] = story;
            }
            return loadedFiles;
        }
    }

   }
