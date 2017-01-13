import { Subscription } from 'rxjs/Subscription';
import { Component } from '@angular/core';
import { Router, RoutesRecognized, ActivatedRouteSnapshot, ActivatedRoute } from '@angular/router';

@Component({
    selector: 'as4-wrapper',
    template: `<div class="wrapper">
        <as4-header></as4-header>
        <as4-sidebar></as4-sidebar>
        <section class="content-wrapper">
            <section class="content-header">
                <h2>{{breadCrumb}}</h2>
            </section>
            <section class="content">
                <router-outlet></router-outlet>
            </section>
        </section>
    </div>`,
    styles: [
        `
            .content-header {
                font-size: 12px !important;
            }

            .content-wrapper {
                height: 100%;
            }
        `
    ]
})
export class WrapperComponent {
    public breadCrumb: string;
    private _routeSubscription: Subscription;
    constructor(private router: Router, private activatedRoute: ActivatedRoute) {
        this.breadCrumb = this.getPath(this.activatedRoute.snapshot);
        this._routeSubscription = this.router
            .events
            .filter(evt => evt instanceof RoutesRecognized)
            .subscribe((result: RoutesRecognized) => {
                this.breadCrumb = this.getPath(result.state.root);
            });
    }
    public ngOnDestroy() {
        if (!!this._routeSubscription) this._routeSubscription.unsubscribe();
    }
    private getPath(activatedRoute: ActivatedRouteSnapshot): string {
        let path = activatedRoute && activatedRoute.data && activatedRoute.data['title'];
        if (!!activatedRoute.firstChild) {
            return this.getPath(activatedRoute.children[0]);
        }

        return path;
    }
}
