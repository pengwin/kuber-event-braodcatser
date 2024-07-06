.PHONY: build-docker

VER=0.0.9

build-docker:
	@echo "Don't forget to run eval $$\(minikube docker-env\)"
	docker build -t event-operator:$(VER) -f Operator/Dockerfile .
	docker build -t event-listener:$(VER) -f Listener/Dockerfile .

deploy:
	kubectl apply -k iac/

delete-deployment:
	kubectl delete -k iac/

run-rabbit:
	docker compose up -d rabbitmq

