Features implementations location:

Feature 1: FoodFast -> FoodFast.API -> Controllers -> AuthController

Feature 2: FoodFast -> FoodFast.API -> Controllers -> OrdersController -> Create and Update Orders Endpoints (in these happens the SSE for Order Status)

Feature 3: FoodFast -> FoodFast.API -> Hubs -> DeliveryHub -> In here the customer access the location of the driver (using SignalR)

Feature 4: FoodFast -> FoodFast.API -> Controllers -> OrdersController -> Create and Update Orders Endpoints (in these happens the SSE for Restaurant Orders Dashboard)

Feature 5: FoodFast -> FoodFast.API -> Hubs -> ChatHub -> In here users send messages ---- FoodFast -> FoodFast.API -> Controllers -> ChatController -> For messages history

Feature 6: FoodFast -> FoodFast.API -> Controllers -> AnnouncementsController -> Creating Announcement which triggers Pub/Sub for sending them to users ---- FoodFast -> FoodFast.API -> Hubs -> AnnouncementHub -> Users joining to announcements

Feature 7: FoodFast -> FoodFast.API -> Controllers -> UploadsController -> Uploading images and checking there status after finishing processing it
