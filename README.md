# ink-grpc-runtime

A gRPC service built around the [ink](https://github.com/inkle/ink) runtime.

Currently the only runtime functions exposed are:

1. Creating new stories
2. Continuing a story
3. Picking a story choice

The [ink-server](https://github.com/awwithro/ink-server) acts as a client to the runtime and 
allows for web frontend to the serivce.

