import { Routes } from '@angular/router';
import { ProductList } from './pages/product-list/product-list';
import { ProductDetail } from './pages/product-detail/product-detail';
import { ProductGroupList } from './pages/product-group-list/product-group-list';
import { PurchaseList } from './pages/purchase-list/purchase-list';
import { PurchaseDetail } from './pages/purchase-detail/purchase-detail';
import { UserList } from './pages/user-list/user-list';
import { UserDetail } from './pages/user-detail/user-detail';
import { Health } from './pages/health/health';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'products' },
  { path: 'products', component: ProductList, title: 'Products' },
  { path: 'products/:id', component: ProductDetail, title: 'Product' },
  { path: 'product-groups', component: ProductGroupList, title: 'Product groups' },
  { path: 'purchases', component: PurchaseList, title: 'Purchases' },
  { path: 'purchases/:id', component: PurchaseDetail, title: 'Purchase' },
  { path: 'users', component: UserList, title: 'Customers' },
  { path: 'users/:id', component: UserDetail, title: 'Customer' },
  { path: 'health', component: Health, title: 'Health' },
  { path: '**', redirectTo: 'products' },
];
