# install minikube



# build docker

```bash
eval $(minikube docker-env)
docker build -t event-operator:0.0.1 -f Operator/Dockerfile .
```

# generate 

```bash
dotnet new tool-manifest
dotnet tool install KubeOps.Cli
dotnet kubeops api-version
dotnet kubeops generate operator event-broadcaster EventBroadcaster.sln
```

# deploy

```bash
cd iac
kubectl apply -k .
```