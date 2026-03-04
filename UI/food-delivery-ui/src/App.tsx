import React from 'react';
import logo from './logo.svg';
import './App.css';
import { CreateOrderComponent } from './components/CreateOrder';
import { DeliveryStoreComponent } from './components/DeliveryStoreComponent';

function App() {
  return (
    <div className='container-fluid' >
      <div className='row'>
        <div className='col-4'>
          <CreateOrderComponent></CreateOrderComponent>
        </div>
        <div className='col-4'>
          
        </div>
        <div className='col-4'>
          <DeliveryStoreComponent></DeliveryStoreComponent>
        </div>
      </div>
      
      
    </div>
  );
}

export default App;
