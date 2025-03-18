# Running in Docker with Visual Studio

## Prerequisites
- Install [Visual Studio](https://visualstudio.microsoft.com/) with the **ASP.NET and web development** workload.
- Install [Docker Desktop](https://www.docker.com/products/docker-desktop/) and ensure it is running.

## Add secrets
Add .env file in the same directory, as docker-compose

## Running the Application in Docker
1. **Open the Project**: Launch Visual Studio and open the ASP.NET Web API project.
2. **Set Docker as Startup Project**:
   - In **Solution Explorer**, right-click on the project.
   - Select **Set as Startup Project**.
   - In the top toolbar, select **Docker** as the run target.
3. **Run the Application**:
   - Press `F5` to start debugging or `Ctrl + F5` to run without debugging.
   - Visual Studio will build the Docker image, start the container, and launch the application.
   - The API should now be accessible at `http://localhost:<port>` (port may vary).
to learn more visit [site](https://learn.microsoft.com/en-us/visualstudio/containers/container-tools?view=vs-2022)