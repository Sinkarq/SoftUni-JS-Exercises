import {html, render} from "../../node_modules/lit-html/lit-html.js";
import {containerElement, throwException, validator} from "../misc.js";
import {Endpoints} from "../API/Endpoints.js";
import page from "//unpkg.com/page/page.mjs";

export const createPage = () => render(createPageView(), containerElement);

const createPageView = () => html`
    <div class="row space-top">
        <div class="col-md-12">
            <h1>Create New Furniture</h1>
            <p>Please fill all fields.</p>
        </div>
    </div>
    <form @submit="${async (e) => await onSubmit(e)}">
        <div class="row space-top">
            <div class="col-md-4">
                <div class="form-group">
                    <label class="form-control-label" for="new-make">Make</label>
                    <input @input="${(e) => validator.must_be_4_long(e.currentTarget)}" class="form-control"
                           id="new-make" type="text"
                           name="make">
                </div>
                <div class="form-group has-success">
                    <label class="form-control-label" for="new-model">Model</label>
                    <input @input="${(e) => validator.must_be_4_long(e.currentTarget)}" class="form-control"
                           id="new-model" type="text" name="model">
                </div>
                <div class="form-group has-danger">
                    <label class="form-control-label" for="new-year">Year</label>
                    <input @input="${(e) => validator.must_be_between_1950_2050(e.currentTarget)}" class="form-control"
                           id="new-year" type="number" name="year">
                </div>
                <div class="form-group">
                    <label class="form-control-label" for="new-description">Description</label>
                    <input @input="${(e) => validator.must_be_more_than_10(e.currentTarget)}" class="form-control"
                           id="new-description" type="text" name="description">
                </div>
            </div>
            <div class="col-md-4">
                <div class="form-group">
                    <label class="form-control-label" for="new-price">Price</label>
                    <input @input="${(e) => validator.must_be_positive_number(e.currentTarget)}" class="form-control"
                           id="new-price" type="number" name="price">
                </div>
                <div class="form-group">
                    <label class="form-control-label" for="new-image">Image</label>
                    <input @input="${(e) => validator.must_not_be_empty(e.currentTarget)}" class="form-control"
                           id="new-image" type="text" name="img">
                </div>
                <div class="form-group">
                    <label class="form-control-label" for="new-material">Material (optional)</label>
                    <input class="form-control" id="new-material" type="text" name="material">
                </div>
                <input type="submit" class="btn btn-primary" value="Create"/>
            </div>
        </div>
    </form>
`;

const onSubmit = async (e) => {
    e.preventDefault();

    const formData = new FormData(e.currentTarget);
    const {make, model, year, description, price, img, material} = Object.fromEntries(formData);

    let isValid = true;

    e.currentTarget.querySelectorAll('input').forEach((input) => {
        if (input.classList.contains('is-invalid')) {
            isValid = false;
        }
    });

    if (!isValid) {
        throwException('Some of the fields are invalid.');
    }
    if (!material) {
        await Endpoints.createFurniture(make, model, year, description, price, img);
    } else {
        await Endpoints.createFurniture(make, model, year, description, price, img, material);
    }

    page.redirect('/home');
};