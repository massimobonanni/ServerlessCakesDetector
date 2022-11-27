# Serverless Cakes Detector
This project is a simple implementation of a Serverless solution that uses Custom Vision (Objects Detection) to crop cakes from an uploaded image.

The solution allows user to upload an image using a HTTP call (POST multi-part), analyzes the image using a Custom Vision Object Detection ([here for documentation](https://learn.microsoft.com/en-us/azure/cognitive-services/Custom-Vision-Service/get-started-build-detector)) project , and for every cakes founded in the image (based on tags setting in the configuration), the solution crop the image of the cake from the original image and creates an image into a storage account.


