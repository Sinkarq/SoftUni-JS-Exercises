import {refreshNavbarState} from "../app.js";
import {html} from "../../node_modules/lit-html/lit-html.js";
import {getUserId} from "../misc.js";
import {Endpoints} from "../API/Endpoints.js";

export const myPage = async (ctx) => {
    refreshNavbarState();

    const furnitures = await Endpoints.getUserFurniture(getUserId());

    ctx.render(myPageView(furnitures));
};

export const myPageView = (furnitures) => html`
    <div class="row space-top">
        <div class="col-md-12">
            <h1>Welcome to Furniture System</h1>
            <p>Select furniture from the catalog to view details.</p>
        </div>
    </div>
    <div class="row space-top">
        ${furnitures.map(x => furnitureTemplate(x._id, x.img, x.description, x.price))}
    </div>
`;

export const furnitureTemplate = (id, img, description, price) => html`
    <div class="col-md-4">
        <div class="card text-white bg-primary">
            <div class="card-body">
                <img src="${img}"/>
                <p>${description}</p>
                <footer>
                    <p>Price: <span>${price} $</span></p>
                </footer>
                <div>
                    <a href="/details/${id}" class="btn btn-info">Details</a>
                </div>
            </div>
        </div>
    </div>
`;