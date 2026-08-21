## Cross-platform Unit Tests
- *Tailored for quick re-runs locally*. I need to run them locally without having to copy them into the Docker-container and rebuild that every time the source code changes -- for speed. Therefore:
    1. source code gets volume-mounted to Docker containers (not baked into images)
    2. build output within the container is saved to a different directory via [ArtifactsPath](./docker-compose-lnx.yml#L19) -- to avoid the output from a container clashing with the output from a local build.

- *CI Test workflows need build output to carry over from one step to the other*. Since source code is built at container run-time (not the image build-time), and it's more straight-forward to start a new container at each step, the output must be saved outside the container (on the host). Therefore, [ArtifactsPath](./docker-compose-lnx.yml#L19) points to a path on the host.