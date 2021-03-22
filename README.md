# Web Navigator

## Build Status

[![Build Status](https://dev.azure.com/jannemattila/jannemattila/_apis/build/status/JanneMattila.web-navigator?branchName=main)](https://dev.azure.com/jannemattila/jannemattila/_build/latest?definitionId=60&branchName=main)
[![Docker Pulls](https://img.shields.io/docker/pulls/jannemattila/catch-the-banana?style=plastic)](https://hub.docker.com/r/jannemattila/web-navigator)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Introduction

Web Navigator is simple craw

## Working with 'Web Navigator'

### How to create image locally

```bash
# Build container image
docker build . -t web-navigator:latest

# Run container using command
docker run --rm -it -e navigateUri=http://localhost:3000 web-navigator:latest
```

### How to run locally

```bash
docker run -it -e navigateUri=http://localhost:3000 jannemattila/web-navigator:latest
``` 

### How to deploy to Azure Kubernetes Service

```bash
# Variables
aks="aks"
resourceGroup="aks-rg"
location="westeurope"

# Login to Azure
az login

# *Explicitly* select your working context
az account set --subscription <YourSubscriptionName>

# Create new resource group
az group create --name $resourceGroup --location $location

# Create new AKS
az aks create --name $aks --resource-group $resourceGroup --node-count 1 --enable-cluster-autoscaler --min-count 1 --max-count 5

# Install kubectl
sudo az aks install-cli

# Get credentials to new cluster
az aks get-credentials --name $aks --resource-group $resourceGroup

# Fetch yaml which contains deployment for web navigator
wget https://raw.githubusercontent.com/JanneMattila/web-navigator/main/web-navigator.yaml

# EDIT "replicas" to match the expected count.
# EDIT "navigateUri" to match target url.

# Deploy web navigator
kubectl apply -f web-navigator.yaml

# Further update deployment scale
kubectl scale deployment web-navigator --replicas=100

# Monitor pod scaling progress
kubectl get deployment web-navigator -w

# Wipe out the resources
az group delete --name $resourceGroup -y
``` 
