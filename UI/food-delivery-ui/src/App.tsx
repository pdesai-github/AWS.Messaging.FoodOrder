import React from 'react';
import logo from './logo.svg';
import './App.css';
import { CreateOrderComponent } from './components/CreateOrder';
import { DeliveryStoreComponent } from './components/DeliveryStoreComponent';
import Shop from './components/Shop';
import DeliveryDLQ from './components/DeliveryDLQ';

function App() {
  return (
    <div className='container-fluid' >
      <div className='row'>
        <div className='col-4'>
          <CreateOrderComponent></CreateOrderComponent>
        </div>
        <div className='col-4'>
          <Shop shopId={'shop-1'} ></Shop>
          <Shop shopId={'shop-2'} ></Shop>
        </div>
        <div className='col-4'>
          <DeliveryStoreComponent></DeliveryStoreComponent>
          <hr />
          <div>
            <DeliveryDLQ></DeliveryDLQ>
          </div>
        </div>
      </div>
      
      
    </div>
  );
}

export default App;
